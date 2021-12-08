using System;
using Characters.Actions;
using Characters.Damages;
using Characters.Utils;
using MessagePipe;
using Sirenix.OdinInspector;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters
{
    [EnumToggleButtons]
    public enum CharacterType
    {
        Player,
        Enemy,
        Obstacle
    }

    public class CharacterCore : SerializedMonoBehaviour, IDamageApplicable, IMegaCrashDamageApplicable,
        IAttackRepellable, ICharacterStateProvider, ICharacterEventProvider, ICharacterInformationProvider,
        ICharacterPickableObjectProvider, IItemPickableByInventory
    {
        [FormerlySerializedAs("name")] [SerializeField]
        private string characterName;

        public string Name => characterName;

        [HorizontalGroup("Portrait")] [SerializeField] [PreviewField(64)]
        private Sprite portraitImage;

        public Sprite PortraitImage => portraitImage;

        [HorizontalGroup("Portrait")] [SerializeField] [PreviewField(64)]
        private Sprite portraitDamagedImage;

        public Sprite PortraitDamagedImage => portraitDamagedImage;

        public IReadOnlyReactiveProperty<float> ChargeRatio { get; private set; }

        public CharacterStatus InitialStatus => initialStatus;

        [Title("Initial Status")] [HideLabel] [SerializeField]
        private CharacterStatus initialStatus;

        public CharacterType CharacterType => _characterType;
        [SerializeField] private CharacterType _characterType;

        #region Reactive Property
        public IReadOnlyReactiveProperty<int> CurrentHealth => currentHealth;
        [ShowInInspector] [ReadOnly] IntReactiveProperty currentHealth = new IntReactiveProperty();

        public IReadOnlyReactiveProperty<float> HealthRatio => healthRatio;
        [ShowInInspector] [ReadOnly] private FloatReactiveProperty healthRatio = new FloatReactiveProperty();

        public IReadOnlyReactiveProperty<bool> IsInvincible => isInvincible;
        [ShowInInspector] [ReadOnly] BoolReactiveProperty isInvincible = new BoolReactiveProperty(false);
        
        [ShowInInspector] [ReadOnly] public IReadOnlyReactiveProperty<bool> IsGrounded => isGrounded;
        BoolReactiveProperty isGrounded = new BoolReactiveProperty();

        [ShowInInspector]
        [ReadOnly]
        public IReadOnlyReactiveProperty<ActionState> CurrentActionState => _currentActionState;

        ActionStateReactiveProperty _currentActionState = new ActionStateReactiveProperty(ActionState.Idle);
        #endregion

        [ShowInInspector] [ReadOnly] public GameObject UnderFootObject => _underFootObject;
        private GameObject _underFootObject = null;
        [ShowInInspector] [ReadOnly] public GameObject PickingObject => _pickingObject;
        private GameObject _pickingObject = null;

        // 地面または障害物(Obstacle)の上なら歩行可能
        private const int GroundLayerMask = 1 << 6 | 1 << 8;


        #region イベント
        public IObservable<DamageData> OnDamaged => _onDamaged;
        private readonly Subject<DamageData> _onDamaged = new Subject<DamageData>();
        public IObservable<CharacterKnockbackData> OnKnockBack => _onKnockBack;
        private readonly Subject<CharacterKnockbackData> _onKnockBack = new Subject<CharacterKnockbackData>();
        public IObservable<DamageData> OnBlowAway => _onBlowAway;
        private readonly Subject<DamageData> _onBlowAway = new Subject<DamageData>();

        public IObservable<CharacterCore> OnBlockSuccessful => _onBlockSuccessful;
        private readonly Subject<CharacterCore> _onBlockSuccessful = new Subject<CharacterCore>();

        public IObservable<CharacterCore> OnRepelled => _onRepelled;
        private readonly Subject<CharacterCore> _onRepelled = new Subject<CharacterCore>();

        public IObservable<Unit> OnFallDownGround => _onFallDownGround;
        private readonly Subject<Unit> _onFallDownGround = new Subject<Unit>();

        public IObservable<ItemController> OnGetPickupItem => _onPickupItem;
        private readonly Subject<ItemController> _onPickupItem = new Subject<ItemController>();

        public IObservable<ItemController> OnThrowPickupItem => _onThrowPickupItem;
        private readonly Subject<ItemController> _onThrowPickupItem = new Subject<ItemController>();

        public IObservable<ItemController> OnLostPickupItem => _onLostPickupItem;
        private readonly Subject<ItemController> _onLostPickupItem = new Subject<ItemController>();
        
        #endregion

        private ICharacterCollisionEventProvider _characterCollisionEventProvider;

        private bool _isMegaCrashDamageApplied = false;

        private bool _isCheckGroundedSkip = false;

        public Guid PreviousDamageId => _previousDamageId;
        private Guid _previousDamageId; // 同じ攻撃が多段ヒットしないように前回ダメージを受けたときのIDを記憶

        private IPublisher<CharacterCore> _characterCorePublisher;
        private IPublisher<DeadData> _deadDataPublisher;

        private IntReactiveProperty _downCounter = new IntReactiveProperty(0);
        private IDisposable _downCounterTimer;

        // ガードの無敵フレーム数.
        [SerializeField] private int guardInvincibleFrameCount = 90;

        /// <summary>
        /// 敵からの攻撃が複数回続いた時に強制ダウンさせるタイマーの継続時間
        /// タイマー中に殴られた時はまたこの時間でリセットされる
        /// </summary>
        [SerializeField] private float downCountContinueDuration;

        /// <summary>
        /// 強制ダウンされるコンボカウント
        /// </summary>
        [SerializeField] private int forcedDownCount = 3;


        // スーパーアーマー中フラグ
        [ShowInInspector] public IReadOnlyReactiveProperty<bool> HaveSuperArmor => _haveSuperArmor;
        private BoolReactiveProperty _haveSuperArmor = new BoolReactiveProperty(false);

        private IPublisher<CameraShakeEventData> _cameraShakeEventPublisher;


        private void Awake()
        {
            _characterCollisionEventProvider = GetComponent<ICharacterCollisionEventProvider>();
            ChargeRatio = CharacterType switch
            {
                CharacterType.Player => GetComponent<CharacterAttacker>().ChargeRatio,
                CharacterType.Enemy => GetComponent<CharacterAttacker>().ChargeRatio,
                CharacterType.Obstacle => new ReactiveProperty<float>(0),
                _ => ChargeRatio
            };

            #region initialize setup

            currentHealth.Value = InitialStatus.MaxHealth;
            healthRatio.Value = 1f;

            #endregion
        }

        private void Start()
        {
            Debug.Log("player core start");

            #region Globalな(投げっぱなし)イベント用のPublisherを取得

            _cameraShakeEventPublisher = GlobalMessagePipe.GetPublisher<CameraShakeEventData>();
            _characterCorePublisher = GlobalMessagePipe.GetPublisher<CharacterCore>();
            _deadDataPublisher = GlobalMessagePipe.GetPublisher<DeadData>();

            #endregion

            #region ステータスの監視
            // 体力が変動したら、割合に変換して通知
            // 体力の割合(healthRatio)は体力バーの表示などで使用
            currentHealth.SkipLatestValueOnSubscribe().Subscribe(x =>
                {
                    Debug.Log($"current health change => {x}", gameObject);
                    healthRatio.Value = x / (float)initialStatus.MaxHealth;
                }
            ).AddTo(this);
            #endregion

            #region 接地

            // 接地チェック.
            this.UpdateAsObservable().Where(_ => !_isCheckGroundedSkip).Subscribe(_ =>
                {
                    var radius = 0.2f;
                    var positionOffset = Vector3.up * 0.18f;

                    var isHit = Physics.CheckSphere(transform.position + positionOffset, radius, GroundLayerMask);

                    isGrounded.Value = isHit;
                }
            ).AddTo(this);

            // ジャンプ中に地面と接した時、着地中状態にする
            isGrounded
                .Where(x => x)
                .Where(_ => _currentActionState.Value == ActionState.Jump ||
                            _currentActionState.Value == ActionState.JumpAttack
                )
                .Subscribe(_ => { ChangeCurrentActionState(ActionState.Landing); }).AddTo(this);

            // ダウン中に地面にぶつかった時
            isGrounded
                .Pairwise()
                .Where(x => x.Previous == false && x.Current == true)
                .Where(_ => _currentActionState.Value == ActionState.Down ||
                            _currentActionState.Value == ActionState.Dead
                )
                .Subscribe(_ =>
                    {
                        // 通知する(煙や効果音用)
                        _onFallDownGround.OnNext(Unit.Default);
                    }
                ).AddTo(this);


            // ジャンプ直後数フレームは接地チェックを行わない.
            _currentActionState.Where(x => x == ActionState.Jump).Subscribe(_ =>
                {
                    _isCheckGroundedSkip = true;
                    isGrounded.Value = false;
                    Observable.TimerFrame(10).Subscribe(_ => _isCheckGroundedSkip = false).AddTo(this);
                }
            ).AddTo(this);

            #endregion
            
            #region ダウン処理
            
            // ダメージ時、吹き飛び判定が存在する場合は一定時間無敵にする.
            OnDamaged.Where(x => x.isBlowAway)
                .Subscribe(_ =>
                {
                    isInvincible.Value = true;
                }).AddTo(this);
            
            // ダウンしたが死亡していない時は立ち上がり処理に移行.
            // 死亡したかどうかは体力をみている.
            _currentActionState
                .Where(x => x == ActionState.Down)
                .Where(_ => currentHealth.Value > 0)
                .Delay(TimeSpan.FromSeconds(1))
                .Subscribe(_ => { _currentActionState.Value = ActionState.Standing; }).AddTo(this);

            // 立ち上がり状態が終了した(Idle状態などに移行した)ら無敵を解除
            _currentActionState
                .Pairwise()
                .Where(x => x.Previous == ActionState.Standing)
                .Where(x => x.Current != ActionState.Standing)
                .Subscribe(_ => { isInvincible.Value = false; }).AddTo(this);
            
            // プレーヤーがダウンまたは死亡したらカメラを揺らす
            CurrentActionState
                .Where(x => x == ActionState.Down || x == ActionState.Dead)
                .Where(_ => CharacterType == CharacterType.Player)
                .Subscribe(_ =>
                    {
                        _cameraShakeEventPublisher.Publish(
                            new CameraShakeEventData(CameraShakeEventData.CameraShakeType.Heavy)
                        );
                    }
                ).AddTo(this);
            #endregion

            #region メガクラ

            // メガクラしたら無敵化
            _currentActionState
                .Where(x => x == ActionState.MegaCrash)
                .Subscribe(_ => { isInvincible.Value = true; }).AddTo(this);
            // メガクラ状態から戻ったら無敵解除
            _currentActionState
                .Pairwise()
                .Where(x => x.Previous == ActionState.MegaCrash)
                .Where(x => x.Current != ActionState.MegaCrash)
                .Subscribe(_ =>
                    {
                        isInvincible.Value = false;
                        _isMegaCrashDamageApplied = false; // メガクラダメージ適用済フラグも解除
                    }
                ).AddTo(this);
            // メガクラが自分にヒットした時のダメージ処理
            // 複数ヒットした時は最初の一回だけ適用
            _characterCollisionEventProvider?.OnMegaCrashCollisionHit.Subscribe(x =>
                {
                    if (_isMegaCrashDamageApplied) return;

                    ApplyMegaCrashDamage(InitialStatus.MegaCrashCost);
                    _isMegaCrashDamageApplied = true;
                }
            ).AddTo(this);

            #endregion


            #region アイテム取得

            // アイテムがプレーヤーの取得範囲にあるかチェック
            // あればそのアイテムを取得可能オブジェクトとして登録
            this.OnTriggerStayAsObservable()
                .Where(x => x.CompareTag($"Item"))
                .Where(_ => _characterType == CharacterType.Player) // プレイヤー以外はアイテムを拾わない
                .Subscribe(x =>
                    {
                        // 既に取得可能オブジェクトがあり、更にオブジェクトを発見した場合、プレーヤーから最も近い位置にあるものを
                        // 取得候補としてセットする
                        if (_underFootObject)
                        {
                            if ((transform.position - x.transform.position).sqrMagnitude <
                                (transform.position - _underFootObject.transform.position).sqrMagnitude)
                            {
                                _underFootObject = x.gameObject;
                            }
                        }
                        else
                        {
                            _underFootObject = x.gameObject;
                        }
                    }
                ).AddTo(this);
            
            // 足元にあるアイテムの取得可能範囲から抜けたかチェック
            this.OnTriggerExitAsObservable()
                .Where(x => x.CompareTag($"Item"))
                .Where(x => Equals(x.gameObject, _underFootObject)) // 取得可能トリガーから抜けたアイテムが取得可能オブジェクトとして登録されているアイテムのとき
                .Subscribe(x => { _underFootObject = null; }).AddTo(this);

            // アイテム取得中は無敵にする
            CurrentActionState.Where(x => x == ActionState.Pickup).Subscribe(_ =>
                {
                    isInvincible.Value = true;

                    // 7フレームでアイテム取得時の効果適用とアイテム削除と無敵解除
                    Observable.TimerFrame(7).Subscribe(_ =>
                        {
                            var itemData = _underFootObject.GetComponent<ItemController>();
                            switch (itemData.Type)
                            {
                                case ItemType.Food:
                                    ApplyHeal(itemData.Value);
                                    Destroy(_underFootObject);
                                    break;
                                // 投げ物アイテム取得処理
                                // 落ちているオブジェクトをそのまま使う
                                case ItemType.Thrower:
                                    _onPickupItem.OnNext(itemData);
                                    _pickingObject = _underFootObject;
                                    _underFootObject = null;
                                    break;
                            }

                            isInvincible.Value = false;
                        }
                    ).AddTo(this);
                }
            ).AddTo(this);

            #endregion

            // 攻撃を弾かれたときの処理
            OnRepelled.Subscribe(x =>
                {
                    if (CurrentActionState.Value == ActionState.JumpAttack)
                    {
                        // ジャンプ攻撃中に弾かれたとき、吹き飛んでダウンする
                        var knockBackData = new CharacterKnockbackData
                        {
                            KnockBackDirection = KnockBack.Forward,
                            IsBlowAway = true,
                            SlidePower = 5f,
                            UpPower = 3f,
                            AttackerPosition = x.transform.position,
                            AttackerFacingDirection = x.transform.forward.normalized
                        };
                        ChangeCurrentActionState(ActionState.Down);
                        _onKnockBack.OnNext(knockBackData);
                    }
                    else
                    {
                        ChangeCurrentActionState(ActionState.Repelled);
                    }
                }
            ).AddTo(this);

            // ダメージ時、一定時間内に複数回ダメージがあったら、強制的にダウンさせるためのタイマー
            // プレーヤーだけ実行
            if (_characterType == CharacterType.Player)
            {
                OnDamaged.Subscribe(_ =>
                    {
                        _downCounter.Value += 1;

                        _downCounterTimer?.Dispose();
                        _downCounterTimer = Observable.Timer(TimeSpan.FromSeconds(downCountContinueDuration)).Subscribe(
                            _ =>
                            {
                                _downCounter.Value = 0;
                            }
                        ).AddTo(this);
                    }
                ).AddTo(this);

                // もしダウンしたらカウンターを初期化
                CurrentActionState.Where(x => x == ActionState.Standing).Subscribe(_ =>
                    {
                        _downCounter.Value = 0; // ダウンカウンターを初期化
                        _downCounterTimer.Dispose();
                    }
                ).AddTo(this);
            }

            OnLostPickupItem.Subscribe(_ => { _pickingObject = null; }).AddTo(this);


            // 死亡時、もしアイテムを持っていたら
            CurrentActionState.Where(x => x == ActionState.Dead).Subscribe(_ =>
                {
                    if (_pickingObject) _pickingObject.GetComponent<ModelFader>().FadeOut();
                }
            ).AddTo(this);

            // Idleステートに戻るかノックバック属性の攻撃を食らったらスーパーアーマーを解除する
            CurrentActionState
                .Where(x =>
                    x == ActionState.Idle ||
                    x == ActionState.Down ||
                    x == ActionState.KnockBack ||
                    x == ActionState.Repelled ||
                    x == ActionState.Dead
                )
                .Subscribe(_ => { _haveSuperArmor.Value = false; }).AddTo(this);

        }

        public void ChangeCurrentActionState(ActionState state)
        {
            if (_currentActionState.Value != state) _currentActionState.Value = state;
        }

        /// <summary>
        /// 地面ではなく自分の懐からアイテムを取り出す
        /// 主に敵の投げナイフ用
        /// </summary>
        public void PickupItemByInventory(GameObject go)
        {
            var itemData = go.GetComponent<ItemController>();
            switch (itemData.Type)
            {
                case ItemType.Food:
                    ApplyHeal(itemData.Value);
                    Destroy(_underFootObject);
                    break;
                // 投げ物アイテム取得処理
                // 落ちているオブジェクトをそのまま使う
                case ItemType.Thrower:
                    _onPickupItem.OnNext(itemData);
                    _pickingObject = go;
                    _underFootObject = null;
                    break;
            }
        }

        public void ThrowItem()
        {
            _onThrowPickupItem.OnNext(_pickingObject.GetComponent<ItemController>());
            _onLostPickupItem.OnNext(_pickingObject.GetComponent<ItemController>());
        }

        public void ApplyHeal(int value)
        {
            var newHealth = currentHealth.Value + value;

            if (newHealth >= InitialStatus.MaxHealth) newHealth = InitialStatus.MaxHealth;

            currentHealth.Value = newHealth;
        }

        public void ApplyMegaCrashDamage(int value)
        {
            var newHealth = currentHealth.Value - value;
            if (newHealth <= 0) newHealth = 1;

            currentHealth.Value = newHealth;
        }

        public void ApplyDamage(DamageData damageData)
        {
            _previousDamageId = damageData.damageId;

            // 被害者が敵ならUIへ被害者情報を送る
            // 被害者がプレーヤーならUIへ攻撃者情報を送る
            //MessageBroker.Default.Publish(damageData.attackerCore._characterType == CharacterType.Player ? this : damageData.attackerCore);
            _characterCorePublisher.Publish(damageData.attackerCore._characterType == CharacterType.Player
                ? this
                : damageData.attackerCore
            );

            // 自分が物で、攻撃タイプがなげものだったら何もしない(攻撃を受けない)
            if (_characterType == CharacterType.Obstacle && damageData.attackType == AttackType.ThrowItem) return;

            // ダメージブロック処理.
            if (_currentActionState.Value == ActionState.Block)
            {
                if (Math.Abs(Mathf.Sign(transform.forward.x) - Mathf.Sign(damageData.attackerCore.transform.forward.x)
                ) > 0.1f)
                {
                    Debug.Log("Block Successful!!", gameObject);
                    _onBlockSuccessful.OnNext(damageData.attackerCore);
                    _currentActionState.Value = ActionState.BlockSuccess;
                    isInvincible.Value = true; // 一定時間無敵化.
                    Observable.TimerFrame(guardInvincibleFrameCount).Subscribe(_ => { isInvincible.Value = false; })
                        .AddTo(this);

                    // ブロック成功した場合はノックバックする
                    var knockBackData = new CharacterKnockbackData
                    {
                        KnockBackDirection = KnockBack.Forward,
                        IsBlowAway = damageData.isBlowAway,
                        SlidePower = 5f,
                        UpPower = 0f,
                        AttackerPosition = damageData.attackerCore.transform.position,
                        AttackerFacingDirection = damageData.attackerCore.transform.forward.normalized
                    };
                    _onKnockBack.OnNext(knockBackData);

                    // もし飛び道具ではなかった場合
                    if (damageData.attackType == AttackType.ThrowItem) return;
                    // 攻撃者には攻撃が弾かれた(Repel)ことを通知する
                    var repellable = damageData.attackerCore as IAttackRepellable;
                    repellable.ApplyRepel(this);
                    return;
                }
            }

            // ブロックされなかった場合はダメージ処理.

            Debug.Log(
                $"{damageData.damageValue} damaged by {damageData.attackerCore.characterName} Knockback:{damageData}"
            );

            var newHealth = currentHealth.Value - damageData.damageValue;

            if (newHealth <= 0)
            {
                currentHealth.Value = 0;
                // 死亡処理.
                Dead(damageData);
            }
            else
            {
                currentHealth.Value = newHealth;

                // もし空中で攻撃を受けたら元々の攻撃に吹き飛び属性がなくても吹き飛び属性をつける
                if (!isGrounded.Value)
                {
                    damageData.knockBack = KnockBack.Forward;
                    damageData.isBlowAway = true;
                }
                // 一定時間内に複数回吹き飛び属性の無い攻撃を食らった場合、吹き飛び属性をつける(永パ防止のため)
                else
                {
                    if (_downCounter.Value >= forcedDownCount && damageData.knockBack == KnockBack.None)
                    {
                        damageData.knockBack = KnockBack.Forward;
                        damageData.isBlowAway = true;
                    }
                }

                // 吹き飛び属性がついていたらダウン、そうじゃなければダメージかノックバック
                if (damageData.isBlowAway)
                    _currentActionState.Value = ActionState.Down;
                else
                {
                    // スーパーアーマー持ちじゃない場合、現在ステートを更新
                    if (!_haveSuperArmor.Value)
                        _currentActionState.Value = damageData.knockBack == KnockBack.None
                            ? ActionState.Damage
                            : ActionState.KnockBack;
                }


                // ダメージイベント発行
                _onDamaged.OnNext(damageData);

                var knockBackData = new CharacterKnockbackData
                {
                    KnockBackDirection = damageData.knockBack,
                    IsBlowAway = damageData.isBlowAway,
                    SlidePower = 6f,
                    UpPower = 4f,
                    AttackerPosition = damageData.attackerCore.transform.position,
                    AttackerFacingDirection = damageData.attackerCore.transform.forward.normalized
                };

                if (knockBackData.KnockBackDirection != KnockBack.None)
                {
                    // ノックバックイベント発行
                    _onKnockBack.OnNext(knockBackData);
                }
            }
        }

        private void Dead(DamageData damageData)
        {
            // 死亡時も吹き飛ぶ.
            _currentActionState.Value = ActionState.Dead;
            // もし非ノックバック攻撃で死んだ場合は
            // 順方向に吹き飛ぶ
            if (damageData.knockBack == KnockBack.None)
                damageData.knockBack = KnockBack.DistanceAway;

            damageData.isBlowAway = true;

            _onDamaged.OnNext(damageData);

            var knockBackData = new CharacterKnockbackData
            {
                KnockBackDirection = damageData.knockBack,
                IsBlowAway = damageData.isBlowAway,
                SlidePower = 5f,
                UpPower = 5f,
                AttackerPosition = damageData.attackerCore.transform.position,
                AttackerFacingDirection = damageData.attackerCore.transform.forward.normalized
            };

            // ノックバックイベント発行
            _onKnockBack.OnNext(knockBackData);

            // 死亡イベント外向けに通知.
            var deadData = new DeadData { characterType = CharacterType, attackerCore = damageData.attackerCore };
            _deadDataPublisher.Publish(deadData);

            isInvincible.Value = true; // 死亡すると無敵化.
            Debug.Log($"{characterName} is Dead! by {damageData.attackerCore.characterName}!");
        }

        public void ApplyRepel(CharacterCore core)
        {
            _onRepelled.OnNext(core);
        }

        /// <summary>
        /// 主にアニメーション中で使用。スーパーアーマーフラグセット処理
        /// </summary>
        /// <param name="isEnable"></param>
        public void SuperArmorOn()
        {
            _haveSuperArmor.Value = true;
        }

        public void SuperArmorOff()
        {
            _haveSuperArmor.Value = false;
        }
    }
}