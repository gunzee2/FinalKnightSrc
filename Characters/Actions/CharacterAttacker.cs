using System;
using Characters.Damages;
using Characters.Inputs;
using Sirenix.OdinInspector;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Characters.Actions
{
    public enum AttackType
    {
        None,
        Combo1,
        Combo2,
        Combo3,
        ChargeAttack,
        JumpAttack1,
        JumpAttack2,
        MegaCrash,
        MoveAttack,
        ThrowItem,
        SlashAttack
    }

    public class CharacterAttacker : SerializedMonoBehaviour, IAttackEventProvider, IAttackParameterProvider
    {
        private ICommandEventProvider _commandEventProvider;
        private ICharacterCollisionEventProvider _characterCollisionEventProvider;
        private ICharacterStateProvider _characterStateProvider;
        private ICharacterInformationProvider _characterInformationProvider;
        private ICharacterPickableObjectProvider _characterPickableObjectProvider;

        private CharacterCore _core;

        public Guid DamageId => _damageId;
        private Guid _damageId;


        public IObservable<AttackType> OnAttack => _onAttack;
        private readonly Subject<AttackType> _onAttack = new Subject<AttackType>();
        public IObservable<Unit> OnChargeStart => _onChargeStart;
        private readonly Subject<Unit> _onChargeStart = new Subject<Unit>();
        public IObservable<Unit> OnChargeEnd => _onChargeEnd;
        private readonly Subject<Unit> _onChargeEnd = new Subject<Unit>();

        public IObservable<Unit> OnBlockCancel => _onBlockCancel;
        private readonly Subject<Unit> _onBlockCancel = new Subject<Unit>();

        // コンボ攻撃のカウントアップを許可するフラグ
        // 攻撃が複数人にヒットしたときにコンボカウントが複数上昇しないように
        // 1攻撃につき1だけカウントアップするようフラグ管理(ヒット自体は複数人にする)
        private bool _isAttackAlreadyHit = false;

        // コンボ継続用タイマー
        // 二回敵を殴って別の敵を殴ってもコンボを継続させるためのもの
        private IDisposable _comboCountTimer;

        private bool _isBlockIntervalUp = true;

        [SerializeField] int maxChargeEnergy = 60;
        [SerializeField] private float comboContinueTime;


        public IReadOnlyReactiveProperty<float> ChargeRatio => _chargeRatio;
        [ShowInInspector] [ReadOnly] private FloatReactiveProperty _chargeRatio = new FloatReactiveProperty(0);

        private int ChargeEnergy
        {
            get => _chargeEnergy;
            set
            {
                _chargeEnergy = value;
                _chargeRatio.Value = (_chargeEnergy / (float)maxChargeEnergy); // float変換する(int同士だと結果もintになる)
            }
        }

        private int _chargeEnergy;

        [ShowInInspector] [ReadOnly] private IntReactiveProperty _comboCount = new IntReactiveProperty(0);

        /// <summary>
        /// 一回のジャンプ中に行ったジャンプ攻撃の回数(ジャンプ攻撃1→2、2→1への移行に使用。1→2→1と3回移行できないようにするため)
        /// </summary>
        [ShowInInspector] [ReadOnly] private IntReactiveProperty _jumpAttackCount = new IntReactiveProperty(0);

        private bool _canBlockAnimationSequence = true;

        private AttackType _previousJumpAttackType = AttackType.None;

        private void Awake()
        {
            _core = GetComponent<CharacterCore>();
            _commandEventProvider = GetComponent<ICommandEventProvider>();
            _characterStateProvider = GetComponent<ICharacterStateProvider>();
            _characterInformationProvider = GetComponent<ICharacterInformationProvider>();
            _characterCollisionEventProvider = GetComponent<ICharacterCollisionEventProvider>();
            _characterPickableObjectProvider = GetComponent<ICharacterPickableObjectProvider>();
        }

        void Start()
        {
            _characterStateProvider
                .CurrentActionState
                .Where(x => x == ActionState.Down)
                .Subscribe(_ => { ResetAttackCombo(); }).AddTo(this);
            _characterStateProvider
                .CurrentActionState
                .Where(x => x == ActionState.Idle)
                .Subscribe(_ => { ResetJumpAttackCount(); }).AddTo(this);

            _characterStateProvider
                .CurrentActionState
                .Where(x => x == ActionState.Tired)
                .Subscribe(_ => { ResetAttackCombo(); }).AddTo(this);

            #region チャージ攻撃関連のストリーム.

            var attackReleaseStream = _commandEventProvider
                .OnCommandInput
                .Where(x => x == InputCommand.AttackRelease)
                .AsUnitObservable();
            var chargeAttackCancelStream = _characterStateProvider.CurrentActionState
                .Where(_ => ChargeEnergy > 0) // チャージ中で.
                .Where(IsChargeAttackCancelTiming) // チャージをキャンセルすべき状態であればキャンセルする.
                .AsUnitObservable();

            attackReleaseStream
                .Where(_ => ChargeEnergy > 0)
                .Subscribe(_ =>
                    {
                        Debug.Log("release!");
                        if (ChargeEnergy == maxChargeEnergy)
                        {
                            Debug.Log("charge attack!");
                            GroundAttack(AttackType.ChargeAttack);
                        }
                        else
                        {
                            AttackCombo();
                        }

                        ChargeEnergy = 0;
                        _onChargeEnd.OnNext(Unit.Default);
                    }
                ).AddTo(this);
            chargeAttackCancelStream
                .Subscribe(_ =>
                    {
                        //Debug.Log("cancel!");

                        _onChargeEnd.OnNext(Unit.Default);

                        ChargeEnergy = 0;
                    }
                ).AddTo(this);

            // チャージ処理.
            _commandEventProvider.OnCommandInput
                .Where(x => x == InputCommand.Attack) // 攻撃ボタンが押されたら.
                .SelectMany(this.UpdateAsObservable()) // Updateの頻度で確認開始(ストリームの変換).
                .DelayFrame(20)
                .Where(_ => CanCharge(_characterStateProvider.CurrentActionState.Value)) // チャージできる状態であればチャージする.
                .Where(_ => _characterPickableObjectProvider.UnderFootObject == null) // 足元に拾えるオブジェクトがない
                .Where(_ => _characterPickableObjectProvider.PickingObject == null) // オブジェクトを手に持っていない
                .TakeUntil(attackReleaseStream.Amb(chargeAttackCancelStream)) // 攻撃ボタンが離されるかチャージがキャンセルされる行動をとった時購読終了する.
                .RepeatUntilDestroy(this) // 終了したらすぐに再購読する.
                .Subscribe(_ =>
                    {
                        // チャージし始めにチャージ開始イベントを発行.
                        if (_chargeEnergy == 0) _onChargeStart.OnNext(Unit.Default);

                        //Debug.Log($"Charging..{ChargeEnergy}");
                        ChargeEnergy += 1;

                        if (ChargeEnergy > maxChargeEnergy) ChargeEnergy = maxChargeEnergy;
                    }
                )
                .AddTo(this);

            #endregion

            // メガクラ.
            _commandEventProvider.OnCommandInput
                .Where(x => x == InputCommand.MegaCrash)
                .Where(_ => IsMegaCrashPlayableActionState())
                .Where(_ => _characterStateProvider.IsGrounded.Value)
                // メガクラする体力が残っている時
                .Where(_ => _characterInformationProvider.CurrentHealth.Value >
                            _characterInformationProvider.InitialStatus.MegaCrashCost
                )
                .Subscribe(_ =>
                    {
                        //Debug.Log($"mega crash - {Time.frameCount} frame");

                        _damageId = Guid.NewGuid();
                        _onAttack.OnNext(AttackType.MegaCrash);
                        _characterStateProvider.ChangeCurrentActionState(ActionState.MegaCrash);
                    }
                ).AddTo(this);

            // 通常攻撃.
            _commandEventProvider
                .OnCommandInput
                .BatchFrame()
                .Where(x => x.Contains(InputCommand.Attack))
                .Where(x => !x.Contains(InputCommand.MegaCrash))
                .Where(_ => IsGroundAttackPlayableActionState())
                .Where(_ => _characterPickableObjectProvider.UnderFootObject == null) // 足元に拾えるオブジェクトがない
                .Where(_ => _characterPickableObjectProvider.PickingObject == null) // オブジェクトを手に持っていない
                .Subscribe(_ => { AttackCombo(); }).AddTo(this);


            // アイテム取得.
            _commandEventProvider
                .OnCommandInput
                .BatchFrame()
                .Where(x => x.Contains(InputCommand.Attack))
                .Where(x => !x.Contains(InputCommand.MegaCrash))
                .Where(_ => IsGroundAttackPlayableActionState())
                .Where(_ => _characterPickableObjectProvider.PickingObject == null) // オブジェクトを手に持っていない
                .Where(_ => _characterPickableObjectProvider.UnderFootObject != null) // 足元に拾えるオブジェクトがある
                .Subscribe(_ => { _characterStateProvider.ChangeCurrentActionState(ActionState.Pickup); }).AddTo(this);

            // アイテムを投げる
            _commandEventProvider
                .OnCommandInput
                .BatchFrame()
                .Where(x => x.Contains(InputCommand.Attack))
                .Where(x => !x.Contains(InputCommand.MegaCrash))
                .Where(_ => IsGroundAttackPlayableActionState())
                .Where(_ => _characterPickableObjectProvider.PickingObject != null) // オブジェクトを手に持っている
                .Subscribe(_ =>
                    {
                        _onAttack.OnNext(AttackType.Combo1);
                        _damageId = Guid.NewGuid();
                        _characterStateProvider.ChangeCurrentActionState(ActionState.ThrowItem);
                    }
                ).AddTo(this);

            // 移動攻撃
            _commandEventProvider
                .OnCommandInput
                .BatchFrame()
                .Where(x => x.Contains(InputCommand.MoveAttack))
                .Where(_ => _characterStateProvider.CurrentActionState.Value == ActionState.Idle)
                .Subscribe(_ =>
                    {
                        _damageId = Guid.NewGuid();
                        _onAttack.OnNext(AttackType.MoveAttack);
                        _characterStateProvider.ChangeCurrentActionState(ActionState.MoveAttack);
                    }
                ).AddTo(this);
            _commandEventProvider
                .OnCommandInput
                .BatchFrame()
                .Where(x => x.Contains(InputCommand.SlashAttack))
                .Where(x => !x.Contains(InputCommand.MegaCrash))
                .Where(_ => IsGroundAttackPlayableActionState())
                .Where(_ => _characterPickableObjectProvider.PickingObject == null) // オブジェクトを手に持っていない
                .Subscribe(_ =>
                    {
                        _damageId = Guid.NewGuid();
                        _onAttack.OnNext(AttackType.SlashAttack);
                        _characterStateProvider.ChangeCurrentActionState(ActionState.MoveAttack);
                    }
                ).AddTo(this);

            #region ジャンプ攻撃

            // ジャンプ攻撃1.
            _commandEventProvider
                .OnCommandInput
                .BatchFrame()
                .Where(x => x.Contains(InputCommand.Attack))
                .Where(x => !x.Contains(InputCommand.DownAttack))
                .Where(_ => CanJumpAttack())
                .Where(_ => _previousJumpAttackType == AttackType.None ||
                            _previousJumpAttackType == AttackType.JumpAttack2
                ) // 前の攻撃タイプが無い(ジャンプ後に攻撃していない)かジャンプ攻撃2ならOK
                .Subscribe(_ =>
                    {
                        Debug.Log("jump attack 1");
                        JumpAttack(AttackType.JumpAttack1);
                    }
                ).AddTo(this);

            // ジャンプ攻撃2.
            _commandEventProvider
                .OnCommandInput
                .Where(x => x == InputCommand.DownAttack) // 下攻撃
                .Where(_ => CanJumpAttack())
                .Where(_ => _previousJumpAttackType == AttackType.None ||
                            _previousJumpAttackType == AttackType.JumpAttack1
                ) // 前の攻撃タイプが無い(ジャンプ後に攻撃していない)かジャンプ攻撃1ならOK
                .Subscribe(_ =>
                    {
                        Debug.Log("jump attack 2");

                        JumpAttack(AttackType.JumpAttack2);
                    }
                ).AddTo(this);

            #endregion

            #region ブロック

            _commandEventProvider
                .OnCommandInput
                .Where(x => x == InputCommand.Block)
                .Where(_ => _isBlockIntervalUp)
                .Where(_ => _characterStateProvider.CurrentActionState.Value == ActionState.GroundAttack ||
                            _characterStateProvider.CurrentActionState.Value == ActionState.Idle ||
                            _characterStateProvider.CurrentActionState.Value == ActionState.Move
                ) // アイドル中もしくは攻撃の出始めのみブロック可能.
                .Where(_ => _canBlockAnimationSequence)
                .Subscribe(_ =>
                    {
                        Debug.Log($"Block", gameObject);
                        _characterStateProvider.ChangeCurrentActionState(ActionState.Block);

                        // ブロック再使用タイマーをON
                        _isBlockIntervalUp = false;
                        Observable.TimerFrame(10).Subscribe(_ => _isBlockIntervalUp = true).AddTo(this);
                    }
                ).AddTo(this);

            _commandEventProvider
                .OnCommandInput
                .BatchFrame()
                .Where(x => !x.Contains(InputCommand.Block))
                .Where(_ => _characterStateProvider.CurrentActionState.Value == ActionState.Block)
                .Subscribe(_ =>
                    {
                        Debug.Log($"Block Cancel");
                        _characterStateProvider.ChangeCurrentActionState(ActionState.BlockCancel);
                        _onBlockCancel.OnNext(Unit.Default);
                    }
                ).AddTo(this);
            _commandEventProvider
                .OnCommandInput
                .BatchFrame()
                .Where(x => x.Contains(InputCommand.AttackRelease))
                .Where(_ => _characterStateProvider.CurrentActionState.Value == ActionState.Block)
                .Subscribe(_ =>
                    {
                        Debug.Log($"Block Cancel");
                        _characterStateProvider.ChangeCurrentActionState(ActionState.BlockCancel);
                        _onBlockCancel.OnNext(Unit.Default);
                    }
                ).AddTo(this);

            // ガード成功したらエネルギーをマックスにする
            // ガードアニメーション中にボタンを離したら自動でチャージ攻撃が発動する
            _core.OnBlockSuccessful.Subscribe(_ =>
                {
                    _onChargeStart.OnNext(Unit.Default);
                    ChargeEnergy = maxChargeEnergy;
                }
            ).AddTo(this);

            #endregion

            #region コリジョンヒット

            foreach (var comboAttackHit in _characterCollisionEventProvider.OnAttackComboCollisionHit)
            {
                comboAttackHit.Subscribe(x =>
                    {
                        // もし既に同じ攻撃を受けていた場合は多段ヒットさせない.
                        if (x.DamageApplicable.PreviousDamageId == _damageId) return;

                        // コンボ状態に応じて反映させるダメージを変える.
                        var damageValue = x.AttackType switch
                        {
                            AttackType.Combo1 => _characterInformationProvider.InitialStatus.Combo1Damage,
                            AttackType.Combo2 => _characterInformationProvider.InitialStatus.Combo2Damage,
                            AttackType.Combo3 => _characterInformationProvider.InitialStatus.Combo3Damage,
                            _ => 0
                        };

                        var isKnockBack = x.AttackType switch
                        {
                            AttackType.Combo1 => KnockBack.None,
                            AttackType.Combo2 => KnockBack.None,
                            AttackType.Combo3 => KnockBack.Forward,
                            _ => KnockBack.None
                        };

                        var isBlowAway = x.AttackType switch
                        {
                            AttackType.Combo1 => false,
                            AttackType.Combo2 => false,
                            AttackType.Combo3 => false,
                            _ => false
                        };


                        Debug.Log($"attack combo collision hit {x.CharacterTag}");

                        DealDamage(x.DamageApplicable, x.AttackType, damageValue, isKnockBack, isBlowAway, _damageId);

                        // TODO これ以下の処理をObservable化すればコリジョン処理を分離できるのではないか
                        // コンボカウントアップ済なら処理を終える
                        if (_isAttackAlreadyHit) return;

                        // 攻撃後最初のヒットならコンボカウントアップし、カウント許可フラグをfalseに
                        _comboCount.Value += 1;
                        Debug.Log($"combo count up => {_comboCount.Value}", gameObject);
                        _isAttackAlreadyHit = true;

                        // コンボがフィニッシュしたら0にリセットする
                        if (_comboCount.Value > 2)
                        {
                            ResetAttackCombo();
                        }
                        else
                        {
                            // コンボ継続ならタイマーをリセットして攻撃待機状態に移行
                            _comboCountTimer?.Dispose();
                            _comboCountTimer = Observable.Timer(TimeSpan.FromSeconds(comboContinueTime)).Subscribe(_ =>
                                {
                                    ResetAttackCombo();
                                }
                            ).AddTo(this);
                        }
                    }
                ).AddTo(this);
            }


            _characterCollisionEventProvider.OnJumpAttackCollisionHit1.Subscribe(x =>
                {
                    DealDamage(
                        x.DamageApplicable, 
                        x.AttackType,
                        _characterInformationProvider.InitialStatus.JumpAttack1Damage, KnockBack.Forward,
                        true,
                        _damageId
                    );
                }
            ).AddTo(this);

            _characterCollisionEventProvider.OnJumpAttackCollisionHit2.Subscribe(x =>
                {
                    DealDamage(
                        x.DamageApplicable, 
                        x.AttackType,
                        _characterInformationProvider.InitialStatus.JumpAttack2Damage, 
                        KnockBack.Reverse, 
                        true,
                        _damageId
                    );
                }
            ).AddTo(this);

            _characterCollisionEventProvider.OnChargeAttackCollisionHit.Subscribe(x =>
                {
                    DealDamage(
                        x.DamageApplicable, 
                        x.AttackType,
                        _characterInformationProvider.InitialStatus.ChargeAttackDamage, 
                        KnockBack.Forward, 
                        true,
                        _damageId
                    );
                }
            ).AddTo(this);
            _characterCollisionEventProvider.OnMegaCrashCollisionHit.Subscribe(x =>
                {
                    DealDamage(
                        x.DamageApplicable, 
                        x.AttackType,
                        _characterInformationProvider.InitialStatus.MegaCrashDamage, 
                        KnockBack.DistanceAway, 
                        true,
                        _damageId
                    );
                }
            ).AddTo(this);

            _characterCollisionEventProvider.OnThrowItemCollisionHit.Subscribe(x =>
                {
                    Debug.Log("throw item collision hit", gameObject);
                    // TODO 後でダメージ等設定する,とりあえず固定値
                    DealDamage(x.DamageApplicable, x.AttackType, 20, KnockBack.DistanceAway, false, _damageId);
                }
            ).AddTo(this);

            #endregion
        }

        private void JumpAttack(AttackType type)
        {
            _damageId = Guid.NewGuid();
            _onAttack.OnNext(type);
            _characterStateProvider.ChangeCurrentActionState(ActionState.JumpAttack);

            _previousJumpAttackType = type;
            _jumpAttackCount.Value++;
        }

        private void AttackCombo()
        {
            // もし前回の攻撃がヒットしなかった場合はコンボカウントをリセットする
            if (!_isAttackAlreadyHit) ResetAttackCombo();

            _isAttackAlreadyHit = false;
            _canBlockAnimationSequence = true; // ブロック可能アニメーションフラグをON(攻撃の当たり判定が出るフレームまでブロックを可能,それ以降を不可能にするため)
            switch (_comboCount.Value)
            {
                case 0:
                    GroundAttack(AttackType.Combo1);
                    break;
                case 1:
                    GroundAttack(AttackType.Combo2);
                    break;
                case 2:
                    GroundAttack(AttackType.Combo3);
                    break;
            }
        }

        private void GroundAttack(AttackType attackType)
        {
            _damageId = Guid.NewGuid();
            _onAttack.OnNext(attackType);
            _characterStateProvider.ChangeCurrentActionState(ActionState.GroundAttack);
        }


        private void ResetAttackCombo()
        {
            Debug.Log("combo count reset", gameObject);
            _comboCount.Value = 0;
            _comboCountTimer?.Dispose();
        }

        public void ResetJumpAttackCount()
        {
            _jumpAttackCount.Value = 0;
            _previousJumpAttackType = AttackType.None;
        }


        private bool IsGroundAttackPlayableActionState()
        {
            if (_characterStateProvider.CurrentActionState.Value == ActionState.Move) return true;
            if (_characterStateProvider.CurrentActionState.Value == ActionState.Idle) return true;
            if (_characterStateProvider.CurrentActionState.Value == ActionState.WaitNextAttack) return true;

            return false;
        }

        private bool IsMegaCrashPlayableActionState()
        {
            if (_characterStateProvider.CurrentActionState.Value == ActionState.Move) return true;
            if (_characterStateProvider.CurrentActionState.Value == ActionState.Idle) return true;
            if (_characterStateProvider.CurrentActionState.Value == ActionState.Damage) return true;

            return false;
        }

        private bool CanJumpAttack()
        {
            if (_characterStateProvider.CurrentActionState.Value != ActionState.Jump &&
                _characterStateProvider.CurrentActionState.Value != ActionState.JumpAttack) return false;
            if (_jumpAttackCount.Value > 1) return false; // 0,1ならOK.

            return true;
        }
        
        private bool CanCharge(ActionState actionState)
        {
            if (actionState == ActionState.Move) return true;
            if (actionState == ActionState.Idle) return true;
            if (actionState == ActionState.Jump) return true;
            if (actionState == ActionState.JumpAttack) return false;
            if (actionState == ActionState.Damage) return false;
            if (actionState == ActionState.GroundAttack) return false;

            return false;
        }

        private bool IsChargeAttackCancelTiming(ActionState actionState)
        {
            if (actionState == ActionState.Move) return false;
            if (actionState == ActionState.Idle) return false;
            if (actionState == ActionState.BlockSuccess) return false;
            if (actionState == ActionState.Landing) return false;

            return true;
        }

        private void DealDamage(IDamageApplicable target, AttackType type, int damageValue, KnockBack knockBack,
            bool isBlowAway, Guid id)
        {
            var damage = new DamageData
            {
                attackerCore = _core, attackType = type, damageValue = damageValue, knockBack = knockBack,
                isBlowAway = isBlowAway, damageId = id
            };
            target.ApplyDamage(damage);
        }

        public void ToWaitNextAttack()
        {
            if (_characterStateProvider.CurrentActionState.Value == ActionState.Block) return;

            _characterStateProvider.ChangeCurrentActionState(ActionState.WaitNextAttack);
        }

        /// <summary>
        /// アニメーション中にブロックを不可能にするためのフラグ。AnimationEventで使用
        /// </summary>
        public void ToCantBlockAnimationSequence()
        {
            _canBlockAnimationSequence = false;
        }
    }
}