using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Actions;
using HC.Debug;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Characters
{
    /// <summary>
    /// 攻撃コリジョン関連の操作
    /// </summary>
    public class CharacterCollisionController : MonoBehaviour, ICharacterCollisionEventProvider
    {
        [SerializeField] private Transform attackPointTransform;

        [SerializeField] private List<Collider> attackComboColliders;
        [SerializeField] private List<Collider> jumpAttackColliders;

        [SerializeField] private Collider chargeAttackCollider;
        [SerializeField] private Collider megaCrashCollider;
        [SerializeField] private ColliderVisualizer.VisualizerColorType _visualizerColor;

        [SerializeField] private bool ShowAttackCollider = false;

        private CharacterCore _core;
        private IAttackParameterProvider _attackParameterProvider;
        private ICharacterEventProvider _characterEventProvider;

        public IObservable<IHitCollisionDataProvider> OnAttackComboCollisionHit1 => _onAttackComboCollisionHit1;

        private readonly Subject<IHitCollisionDataProvider> _onAttackComboCollisionHit1 =
            new Subject<IHitCollisionDataProvider>();

        public IObservable<IHitCollisionDataProvider> OnAttackComboCollisionHit2 => _onAttackComboCollisionHit2;

        private readonly Subject<IHitCollisionDataProvider> _onAttackComboCollisionHit2 =
            new Subject<IHitCollisionDataProvider>();

        public IObservable<IHitCollisionDataProvider> OnAttackComboCollisionHit3 => _onAttackComboCollisionHit3;

        private readonly Subject<IHitCollisionDataProvider> _onAttackComboCollisionHit3 =
            new Subject<IHitCollisionDataProvider>();

        public IObservable<IHitCollisionDataProvider> OnJumpAttackCollisionHit1 => _onJumpAttackCollisionHit1;

        private readonly Subject<IHitCollisionDataProvider> _onJumpAttackCollisionHit1 =
            new Subject<IHitCollisionDataProvider>();

        public IObservable<IHitCollisionDataProvider> OnJumpAttackCollisionHit2 => _onJumpAttackCollisionHit2;

        private readonly Subject<IHitCollisionDataProvider> _onJumpAttackCollisionHit2 =
            new Subject<IHitCollisionDataProvider>();

        public IObservable<IHitCollisionDataProvider> OnChargeAttackCollisionHit => _onChargeAttackCollisionHit;

        private readonly Subject<IHitCollisionDataProvider> _onChargeAttackCollisionHit =
            new Subject<IHitCollisionDataProvider>();

        public IObservable<IHitCollisionDataProvider> OnMegaCrashCollisionHit => _onMegaCrashCollisionHit;

        private readonly Subject<IHitCollisionDataProvider> _onMegaCrashCollisionHit =
            new Subject<IHitCollisionDataProvider>();

        public IObservable<IHitCollisionDataProvider> OnThrowItemCollisionHit => _onThrowItemCollisionHit;

        private readonly Subject<IHitCollisionDataProvider> _onThrowItemCollisionHit =
            new Subject<IHitCollisionDataProvider>();

        public IObservable<Unit> OnThrowItemCollisionDrop => _onThrowItemCollisionDrop;
        private readonly Subject<Unit> _onThrowItemCollisionDrop = new Subject<Unit>();

        public List<IObservable<IHitCollisionDataProvider>> OnAttackComboCollisionHit => _onAttackComboCollisionHit;

        private List<IObservable<IHitCollisionDataProvider>> _onAttackComboCollisionHit;

        public IObservable<IHitCollisionDataProvider> OnAttackCollisionHit => _onAttackCollisionHit;

        private readonly Subject<IHitCollisionDataProvider> _onAttackCollisionHit =
            new Subject<IHitCollisionDataProvider>();


        private void Awake()
        {
            _core = GetComponent<CharacterCore>();
            _attackParameterProvider = GetComponent<IAttackParameterProvider>();
            _characterEventProvider = GetComponent<ICharacterEventProvider>();

            _onAttackComboCollisionHit = new List<IObservable<IHitCollisionDataProvider>>
            {
                OnAttackComboCollisionHit1, OnAttackComboCollisionHit2, OnAttackComboCollisionHit3
            };
        }

        private void Start()
        {
            // 投げ物
            _characterEventProvider.OnGetPickupItem.Where(x => x.Type == ItemType.Thrower).Subscribe(
                itemDataContainer =>
                {
                    var disposables = new CompositeDisposable();
                    // 拾った時にコライダーを登録
                    var hitDisposable = itemDataContainer.AttackCollider.OnTriggerEnterAsObservable()
                        .Where(x => x.CompareTag("Hitbox"))
                        .Select(x =>
                            {
                                var hitCollisionDataProvider = x.GetComponent<IHitCollisionDataProvider>();
                                hitCollisionDataProvider.HitPosition =
                                    x.ClosestPoint(itemDataContainer.transform.position); // 衝突位置を記録
                                return hitCollisionDataProvider; // ColliderからIHitCollisionDataContainerに射影
                            }
                        )
                        .Where(x => IsDamageApplicableObjectTag(x.CharacterTag))
                        .Where(x => !Equals(x.DamageApplicable.PreviousDamageId, _attackParameterProvider.DamageId))
                        .Where(x => !x.DamageApplicable.IsInvincible.Value) // 当たった対象が無敵状態で無い時.
                        .Where(x => !_core.CompareTag(x.CharacterTag)) // 自分と同じタグじゃなかった場合(プレーヤーはプレーヤー同士,敵は敵同士攻撃しない)
                        .Subscribe(x =>
                            {
                                x.AttackType = AttackType.ThrowItem;
                                _onAttackCollisionHit.OnNext(x);
                                _onThrowItemCollisionHit.OnNext(x);
                                Destroy(itemDataContainer.gameObject);
                            }
                        )
                        .AddTo(itemDataContainer.gameObject);
                    // 壁やオブジェクトに当たった時にアイテムを落としてコライダーを破棄
                    var dropDisposable = itemDataContainer.AttackCollider.OnTriggerEnterAsObservable()
                        .Where(x => x.CompareTag("CameraTrigger") || x.CompareTag("Obstacle") ||
                                    x.CompareTag("AttackCollision")
                        )
                        .Subscribe(x =>
                            {
                                itemDataContainer.DropItem();
                                _onThrowItemCollisionDrop.OnNext(Unit.Default);
                                disposables.Clear();
                            }
                        )
                        .AddTo(itemDataContainer.gameObject);

                    disposables.Add(hitDisposable);
                    disposables.Add(dropDisposable);
                    disposables.AddTo(itemDataContainer);
                }
            ).AddTo(this);

            #region 攻撃コリジョン関連.

            // 攻撃コライダーが敵に触れた場合、ダメージ処理.
            foreach (var (col, index) in attackComboColliders.Select((v, i) => (v, i)))
            {
                col.OnTriggerEnterAsObservable()
                    .Where(x => x.CompareTag("Hitbox"))
                    .Select(x =>
                        {
                            var hitCollisionDataProvider = x.GetComponent<IHitCollisionDataProvider>();
                            hitCollisionDataProvider.HitPosition =
                                x.ClosestPoint(attackPointTransform.position); // 衝突位置を記録
                            return hitCollisionDataProvider; // ColliderからIHitCollisionDataContainerに射影
                        }
                    )
                    .Where(x => IsDamageApplicableObjectTag(x.CharacterTag))
                    .Where(x => !Equals(x.DamageApplicable.PreviousDamageId, _attackParameterProvider.DamageId))
                    .Where(x => !x.DamageApplicable.IsInvincible.Value) // 当たった対象が無敵状態で無い時.
                    .Where(x => !_core.CompareTag(x.CharacterTag)) // 自分と同じタグじゃなかった場合(プレーヤーはプレーヤー同士,敵は敵同士攻撃しない)
                    .Subscribe(x =>
                        {
                            _onAttackCollisionHit.OnNext(x);
                            switch (index)
                            {
                                case 0:
                                    x.AttackType = AttackType.Combo1;
                                    _onAttackComboCollisionHit1.OnNext(x);
                                    break;
                                case 1:
                                    x.AttackType = AttackType.Combo2;
                                    _onAttackComboCollisionHit2.OnNext(x);
                                    break;
                                case 2:
                                    x.AttackType = AttackType.Combo3;
                                    _onAttackComboCollisionHit3.OnNext(x);
                                    break;
                            }
                        }
                    ).AddTo(this);
            }

            foreach (var (col, index) in jumpAttackColliders.Select((v, i) => (v, i)))
            {
                col.OnTriggerEnterAsObservable()
                    .Where(x => x.CompareTag("Hitbox"))
                    .Select(x =>
                        {
                            var hitCollisionDataProvider = x.GetComponent<IHitCollisionDataProvider>();
                            hitCollisionDataProvider.HitPosition =
                                x.ClosestPoint(attackPointTransform.position); // 衝突位置を記録
                            return hitCollisionDataProvider; // ColliderからIHitCollisionDataContainerに射影
                        }
                    )
                    .Where(x => IsDamageApplicableObjectTag(x.CharacterTag))
                    .Where(x => !Equals(x.DamageApplicable.PreviousDamageId, _attackParameterProvider.DamageId))
                    .Where(x => !x.DamageApplicable.IsInvincible.Value) // 当たった対象が無敵状態で無い時.
                    .Where(x => !_core.CompareTag(x.CharacterTag)) // 自分と同じタグじゃなかった場合(プレーヤーはプレーヤー同士,敵は敵同士攻撃しない)
                    .Subscribe(x =>
                        {
                            _onAttackCollisionHit.OnNext(x);
                            if (index == 0)
                            {
                                Debug.Log("jump attack hit");
                                x.AttackType = AttackType.JumpAttack1;
                                _onJumpAttackCollisionHit1.OnNext(x);
                            }
                            else
                            {
                                Debug.Log("jump attack hit2");
                                x.AttackType = AttackType.JumpAttack2;
                                _onJumpAttackCollisionHit2.OnNext(x);
                            }
                        }
                    ).AddTo(this);
            }

            chargeAttackCollider.OnTriggerEnterAsObservable()
                .Where(x => x.CompareTag("Hitbox"))
                .Select(x =>
                    {
                        var hitCollisionDataProvider = x.GetComponent<IHitCollisionDataProvider>();
                        hitCollisionDataProvider.HitPosition = x.ClosestPoint(attackPointTransform.position); // 衝突位置を記録
                        return hitCollisionDataProvider; // ColliderからIHitCollisionDataContainerに射影
                    }
                )
                .Where(x => IsDamageApplicableObjectTag(x.CharacterTag))
                .Where(x => !Equals(x.DamageApplicable.PreviousDamageId, _attackParameterProvider.DamageId))
                .Where(x => !x.DamageApplicable.IsInvincible.Value) // 当たった対象が無敵状態で無い時.
                .Where(x => !_core.CompareTag(x.CharacterTag)) // 自分と同じタグじゃなかった場合(プレーヤーはプレーヤー同士,敵は敵同士攻撃しない)
                .Subscribe(x =>
                    {
                        x.AttackType = AttackType.ChargeAttack;
                        _onAttackCollisionHit.OnNext(x);
                        _onChargeAttackCollisionHit.OnNext(x);
                    }
                ).AddTo(this);
            megaCrashCollider.OnTriggerEnterAsObservable()
                .Where(x => x.CompareTag("Hitbox"))
                .Select(x =>
                    {
                        var hitCollisionDataProvider = x.GetComponent<IHitCollisionDataProvider>();
                        hitCollisionDataProvider.HitPosition = x.ClosestPoint(attackPointTransform.position); // 衝突位置を記録
                        return hitCollisionDataProvider; // ColliderからIHitCollisionDataContainerに射影
                    }
                )
                .Where(x => IsDamageApplicableObjectTag(x.CharacterTag))
                .Where(x => !Equals(x.DamageApplicable.PreviousDamageId, _attackParameterProvider.DamageId))
                .Where(x => !x.DamageApplicable.IsInvincible.Value) // 当たった対象が無敵状態で無い時.
                .Where(x => !_core.CompareTag(x.CharacterTag)) // 自分と同じタグじゃなかった場合(プレーヤーはプレーヤー同士,敵は敵同士攻撃しない)
                .Subscribe(x =>
                    {
                        x.AttackType = AttackType.MegaCrash;
                        _onAttackCollisionHit.OnNext(x);
                        _onMegaCrashCollisionHit.OnNext(x);
                    }
                ).AddTo(this);

            // 攻撃中にダメージを食らったらコリジョンを消す
            // またはIdleステートに戻ったらコリジョンを消す
            // アニメーション内でコリジョンのON/OFFを行っているため、ONした後ダメージで割り込まれるとコリジョンが消えない可能性があるため
            _core.CurrentActionState
                .Where(x => x == ActionState.Damage || x == ActionState.Down || x == ActionState.Idle ||
                            x == ActionState.KnockBack || x == ActionState.Dead
                )
                .Subscribe(_ => { DisableAllAttackCollision(); }).AddTo(this);

            #endregion
        }

        public void EnableAttackCollision(AttackType attackType)
        {
            // 途中でダメージ処理などに割り込まれたりした場合はEnableしない
            if (_core.CurrentActionState.Value != ActionState.GroundAttack &&
                _core.CurrentActionState.Value != ActionState.JumpAttack &&
                _core.CurrentActionState.Value != ActionState.MoveAttack &&
                _core.CurrentActionState.Value != ActionState.MegaCrash &&
                _core.CurrentActionState.Value != ActionState.WaitNextAttack
            ) return;
            Debug.Log($"current action state is {_core.CurrentActionState.Value}");
            // コリジョンを表示してから消すまでの間に再度攻撃が行われた場合のために念の為全てのコリジョンを消す
            DisableAllAttackCollision();

            switch (attackType)
            {
                case AttackType.Combo1:
                    attackComboColliders[0].enabled = true;
                    Debug.Log("attack combo1 enabled");
                    EnableAttackColliderVisualizer(attackComboColliders[0], _visualizerColor, "Combo1", 36);
                    break;
                case AttackType.Combo2:
                    attackComboColliders[1].enabled = true;
                    EnableAttackColliderVisualizer(attackComboColliders[1], _visualizerColor, "Combo2", 36);
                    break;
                case AttackType.Combo3:
                    attackComboColliders[2].enabled = true;
                    EnableAttackColliderVisualizer(attackComboColliders[2], _visualizerColor, "Combo3", 36);
                    break;
                case AttackType.JumpAttack1:
                    jumpAttackColliders[0].enabled = true;
                    EnableAttackColliderVisualizer(jumpAttackColliders[0], _visualizerColor, "Jump1", 36);
                    break;
                case AttackType.JumpAttack2:
                    jumpAttackColliders[1].enabled = true;
                    EnableAttackColliderVisualizer(jumpAttackColliders[1], _visualizerColor, "Jump2", 36);
                    break;
                case AttackType.ChargeAttack:
                    chargeAttackCollider.enabled = true;
                    EnableAttackColliderVisualizer(chargeAttackCollider, _visualizerColor, "Charge", 36);
                    break;
                case AttackType.MegaCrash:
                    megaCrashCollider.enabled = true;
                    EnableAttackColliderVisualizer(megaCrashCollider, _visualizerColor, "MegaCrash", 36);
                    break;
            }
        }

        public void DisableAttackCollision(AttackType attackType)
        {
            switch (attackType)
            {
                case AttackType.Combo1:
                    attackComboColliders[0].enabled = false;
                    Debug.Log("attack combo1 disabled");
                    DisableAttackColliderVisualizer(attackComboColliders[0]);
                    break;
                case AttackType.Combo2:
                    attackComboColliders[1].enabled = false;
                    DisableAttackColliderVisualizer(attackComboColliders[1]);
                    break;
                case AttackType.Combo3:
                    attackComboColliders[2].enabled = false;
                    DisableAttackColliderVisualizer(attackComboColliders[2]);
                    break;
                case AttackType.JumpAttack1:
                    jumpAttackColliders[0].enabled = false;
                    DisableAttackColliderVisualizer(jumpAttackColliders[0]);
                    break;
                case AttackType.JumpAttack2:
                    jumpAttackColliders[1].enabled = false;
                    DisableAttackColliderVisualizer(jumpAttackColliders[1]);
                    break;
                case AttackType.ChargeAttack:
                    chargeAttackCollider.enabled = false;
                    DisableAttackColliderVisualizer(chargeAttackCollider);
                    break;
                case AttackType.MegaCrash:
                    megaCrashCollider.enabled = false;
                    DisableAttackColliderVisualizer(megaCrashCollider);
                    break;
            }
        }

        public void DisableAllAttackCollision()
        {
            if (attackComboColliders[0].enabled)
                DisableAttackCollision(AttackType.Combo1);
            if (attackComboColliders[1].enabled)
                DisableAttackCollision(AttackType.Combo2);
            if (attackComboColliders[2].enabled)
                DisableAttackCollision(AttackType.Combo3);
            if (jumpAttackColliders[0].enabled)
                DisableAttackCollision(AttackType.JumpAttack1);
            if (jumpAttackColliders[1].enabled)
                DisableAttackCollision(AttackType.JumpAttack2);
            if (chargeAttackCollider.enabled)
                DisableAttackCollision(AttackType.ChargeAttack);
            if (megaCrashCollider.enabled)
                DisableAttackCollision(AttackType.MegaCrash);
        }

        private void EnableAttackColliderVisualizer(Collider targetCollider,
            ColliderVisualizer.VisualizerColorType color, string msg, int fontSize)
        {
            if (!ShowAttackCollider) return;
            if (targetCollider.GetComponent<ColliderVisualizer>() != null) return;

            targetCollider.gameObject.AddComponent<ColliderVisualizer>().Initialize(color, msg, fontSize);
        }

        private void DisableAttackColliderVisualizer(Collider targetCollider)
        {
            if (!ShowAttackCollider) return;
            if (targetCollider.GetComponent<ColliderVisualizer>() == null) return;

            Destroy(targetCollider.GetComponent<ColliderVisualizer>());
        }

        private bool IsDamageApplicableObjectTag(string characterTag)
        {
            switch (characterTag)
            {
                case "Player":
                case "Enemy":
                case "Obstacle":
                    return true;
                default:
                    return false;
            }
        }
    }
}