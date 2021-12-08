using System;
using Characters.Actions;
using Characters.Damages;
using Chronos;
using MessagePipe;
using UI;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Utility;

namespace Characters.Animations
{
    public class CharacterAnimator : MonoBehaviour,IAnimationFlagProvider
    {
        private CharacterCore _core;
        
        private IMoveEventProvider _moveEventProvider;
        private IAttackEventProvider _attackEventProvider;
        private IAttackParameterProvider _attackParameterProvider;
        private ICharacterEventProvider _characterEventProvider;
        private CharacterCollisionController _collisionController;


        [SerializeField] private Animator _animator;
    
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int IsAnalogueMoving = Animator.StringToHash("IsAnalogueMoving");
        private static readonly int Combo1Trigger = Animator.StringToHash("Combo1Trigger");
        private static readonly int Combo2Trigger = Animator.StringToHash("Combo2Trigger");
        private static readonly int Combo3Trigger = Animator.StringToHash("Combo3Trigger");
        

        public IReadOnlyReactiveProperty<bool> IsAttackAnimationPlaying => isAttackAnimationPlaying;
        private BoolReactiveProperty isAttackAnimationPlaying = new BoolReactiveProperty(false);
        
        private static readonly int IsJumping = Animator.StringToHash("IsJumping");
        private static readonly int MegaCrashTrigger = Animator.StringToHash("MegaCrashTrigger");
        private static readonly int JumpAttack1Trigger = Animator.StringToHash("JumpAttack1Trigger");
        private static readonly int IsDamaged = Animator.StringToHash("IsDamaged");
        private static readonly int IsDown = Animator.StringToHash("IsDown");
        private static readonly int ChargeAttackTrigger = Animator.StringToHash("ChargeAttackTrigger");
        private static readonly int DamageTrigger = Animator.StringToHash("DamageTrigger");
        private static readonly int BlockTrigger = Animator.StringToHash("BlockTrigger");
        private static readonly int BlockSuccessTrigger = Animator.StringToHash("BlockSuccessTrigger");
        private static readonly int BlockCancelTrigger = Animator.StringToHash("BlockCancelTrigger");
        private static readonly int JumpAttack2Trigger = Animator.StringToHash("JumpAttack2Trigger");
        private static readonly int PickupTrigger = Animator.StringToHash("PickupTrigger");
        private static readonly int MoveAttackTrigger = Animator.StringToHash("MoveAttackTrigger");
        private static readonly int RepelTrigger = Animator.StringToHash("RepelTrigger");
        
        private static readonly int HaveThrowableItem = Animator.StringToHash("HaveThrowableItem");
        private static readonly int IsDead = Animator.StringToHash("IsDead");
        
        private IPublisher<CameraShakeEventData> _cameraShakeEventPublisher;
        private static readonly int IsStepMove = Animator.StringToHash("IsStepMove");
        private static readonly int SlashAttackTrigger = Animator.StringToHash("SlashAttackTrigger");

        private void Awake()
        {
            _core = GetComponent<CharacterCore>();
            _moveEventProvider = GetComponent<IMoveEventProvider>();
            _attackEventProvider = GetComponent<IAttackEventProvider>();
            _attackParameterProvider = GetComponent<IAttackParameterProvider>();
            _characterEventProvider = GetComponent<ICharacterEventProvider>();
            _collisionController = GetComponent<CharacterCollisionController>();

        }

        private void Start()
        {
            _animator.SetBool(IsStepMove,  _moveEventProvider.IsStepMove);
            
            _cameraShakeEventPublisher = GlobalMessagePipe.GetPublisher<CameraShakeEventData>();
            _moveEventProvider.IsMoving.Subscribe(x =>
            {
                //Debug.Log($"Animator IsMoving => {x}", gameObject);
                _animator.SetBool(IsMoving, x);
            }).AddTo(this);
            _moveEventProvider.IsAnalogueMoving.Subscribe(x =>
            {
                _animator.SetBool(IsAnalogueMoving, x);
            }).AddTo(this);

            #region ジャンプアニメーションフラグ処理.
            _core.CurrentActionState
                .Where(x => x == ActionState.Jump).Subscribe(_ =>
                {
                    Debug.Log("is jump on", gameObject);
                    _animator.SetBool(IsJumping, true);
                }).AddTo(this);
            // 前フレームまでジャンプ中で、現在フレームではジャンプ中じゃなくなった場合、ジャンプ中フラグをOFFにする.
            // ジャンプ中＝ジャンプ、ジャンプ攻撃状態.
            _core.CurrentActionState
                .Pairwise()
                .Where(x => IsJumpingState(x.Previous) && !IsJumpingState(x.Current))
                .Subscribe(_ =>
                {
                    Debug.Log("is jump off", gameObject);
                    _animator.SetBool(IsJumping, false);
                }).AddTo(this);
            #endregion

            #region ダメージアニメーションフラグ処理.

            _characterEventProvider.OnDamaged.Where(_ => _core.CurrentActionState.Value == ActionState.Damage).Subscribe(_ =>
            {
                Debug.Log("Damage Animation Start", gameObject);
                
                _animator.ResetTrigger(DamageTrigger);
                _animator.SetTrigger(DamageTrigger);
            }).AddTo(this);
            
            _core.CurrentActionState.Where(x => x == ActionState.KnockBack).Subscribe(_ =>
            {
                Debug.Log("Knockback Animation Start", gameObject);
                
                _animator.ResetTrigger(DamageTrigger);
                _animator.SetTrigger(DamageTrigger);
            }).AddTo(this);
            
            _core.CurrentActionState
                .Pairwise()
                .Where(x => x.Previous == ActionState.Damage && x.Current != ActionState.Damage)
                .Subscribe(_ =>
                {
                    _animator.SetBool(IsDamaged, false);
                }).AddTo(this);
            _core.CurrentActionState
                .Pairwise()
                .Where(x => x.Previous == ActionState.KnockBack && x.Current != ActionState.KnockBack)
                .Subscribe(_ =>
                {
                    _animator.SetBool(IsDamaged, false);
                }).AddTo(this);
            #endregion
            
            _core.CurrentActionState.Where(x => x == ActionState.Block).Subscribe(_ =>
            {
                _animator.SetTrigger(BlockTrigger);
            }).AddTo(this);

            _core.CurrentActionState.Where(x => x == ActionState.Down).Subscribe(_ =>
            {
                _animator.SetBool(IsDown, true);
            }).AddTo(this);
            _core.CurrentActionState.Where(x => x == ActionState.Standing).Subscribe(_ =>
            {
                _animator.SetBool(IsDown, false);
            }).AddTo(this);

            _core.CurrentActionState.Where(x => x == ActionState.Pickup).Subscribe(_ =>
            {
                _animator.SetTrigger(PickupTrigger);
            }).AddTo(this);

            _core.CurrentActionState.Where(x => x == ActionState.Repelled).Subscribe(_ =>
            {
                _animator.SetTrigger(RepelTrigger);
            }).AddTo(this);
            _core.CurrentActionState.Where(x => x == ActionState.Dead).Subscribe(_ =>
            {
                _animator.SetBool(IsDead, true);
            }).AddTo(this);
            

            _core.OnBlockSuccessful.Subscribe(_ =>
            {
                _animator.SetTrigger(BlockSuccessTrigger);
            }).AddTo(this);
            
            _attackEventProvider.OnAttack.Subscribe(x =>
            {
                _animator.ResetTrigger(Combo2Trigger);
                _animator.ResetTrigger(Combo3Trigger);
                switch (x)
                {
                    case AttackType.Combo1:
                        _animator.SetTrigger(Combo1Trigger);
                        break;
                    case AttackType.Combo2:
                        _animator.SetTrigger(Combo2Trigger);
                        break;
                    case AttackType.Combo3:
                        _animator.SetTrigger(Combo3Trigger);
                        break;
                    case AttackType.ChargeAttack:
                        _animator.SetTrigger(ChargeAttackTrigger);
                        break;
                    case AttackType.JumpAttack1:
                        _animator.SetTrigger(JumpAttack1Trigger);
                        break;
                    case AttackType.JumpAttack2:
                        _animator.SetTrigger(JumpAttack2Trigger);
                        break;
                    case AttackType.MegaCrash:
                        _animator.SetTrigger(MegaCrashTrigger);
                        break;
                    case AttackType.MoveAttack:
                        _animator.SetTrigger(MoveAttackTrigger);
                        break;
                    case AttackType.SlashAttack:
                        _animator.SetTrigger(SlashAttackTrigger);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(x), x, null);
                }
            }).AddTo(this);

            _attackEventProvider.OnChargeStart.Subscribe(_ =>
            {
                _animator.SetLayerWeight(_animator.GetLayerIndex("RightHandLayer"), 1); // 右手のブレンドを有効化する.
            }).AddTo(this);

            _attackParameterProvider.ChargeRatio.Subscribe(x =>
            {
                _animator.SetFloat("ChargeValue", x); 
                
            }).AddTo(this);
            
            _attackEventProvider.OnChargeEnd.Subscribe(_ =>
            {
                _animator.SetLayerWeight(_animator.GetLayerIndex("RightHandLayer"), 0); // 右手のブレンドを無効化する.
                _animator.SetFloat("ChargeValue", 0); 
            }).AddTo(this);

            _attackEventProvider.OnBlockCancel.Subscribe(_ =>
            {
                _animator.ResetTrigger(BlockCancelTrigger);
                _animator.SetTrigger(BlockCancelTrigger);
            }).AddTo(this);

            _characterEventProvider.OnGetPickupItem.Where(x => x.Type == ItemType.Thrower).Subscribe(_ =>
            {
                _animator.SetBool(HaveThrowableItem, true);
            }).AddTo(this);
            _characterEventProvider.OnLostPickupItem.Where(x => x.Type == ItemType.Thrower).Subscribe(_ =>
            {
                _animator.SetBool(HaveThrowableItem, false);
            }).AddTo(this);


            #region ObservableStateMachineTrigger
            var stateMachineTrigger = _animator.GetBehaviour<ObservableStateMachineTrigger>();

            // ステートを抜けたらトリガーをリセットする(残留する時があるため)
            stateMachineTrigger.OnStateExitAsObservable()
                .Where(x => x.StateInfo.IsName("Combo2"))
                .Subscribe(x =>
                {
                        _animator.ResetTrigger(Combo2Trigger);
                }).AddTo(this);
            stateMachineTrigger.OnStateExitAsObservable()
                .Where(x => x.StateInfo.IsName("Combo3"))
                .Subscribe(x =>
                {
                        _animator.ResetTrigger(Combo3Trigger);
                }).AddTo(this);
            stateMachineTrigger.OnStateExitAsObservable()
                .Where(x => x.StateInfo.IsName("JumpAttack1"))
                .Subscribe(x =>
                {
                        _animator.ResetTrigger(JumpAttack1Trigger);
                }).AddTo(this);
            stateMachineTrigger.OnStateExitAsObservable()
                .Where(x => x.StateInfo.IsName("JumpAttack2"))
                .Subscribe(x =>
                {
                        _animator.ResetTrigger(JumpAttack2Trigger);
                }).AddTo(this);
            
            stateMachineTrigger.OnStateEnterAsObservable()
                .Where(x => x.StateInfo.IsName("Idle"))
                .Subscribe(x =>
                {
                    if (_animator.GetBool(IsJumping)) return;
                    
                    Debug.Log("to Animation Idle State", gameObject);
                    // Idleアニメーションに戻ったらキャラクターの状態もIdle状態に戻す.
                    _core.ChangeCurrentActionState(ActionState.Idle);
                    _animator.ResetTrigger(BlockTrigger);
                    _animator.ResetTrigger(JumpAttack1Trigger);
                    _animator.ResetTrigger(JumpAttack2Trigger);
                    
                    // Idle状態に戻ったら移動フラグをオフにする
                    // 敵が移動中にダウンした場合、IsMovingがfalseにならないため
                    _animator.SetBool(IsMoving, false); 
                }).AddTo(this);
            stateMachineTrigger.OnStateEnterAsObservable()
                .Where(x => x.StateInfo.IsName("Tired"))
                .Subscribe(x =>
                {
                    _core.ChangeCurrentActionState(ActionState.Tired);
                    _animator.ResetTrigger(BlockTrigger);
                }).AddTo(this);
            
            stateMachineTrigger.OnStateExitAsObservable()
                .Where(x => IsComboState(x.StateInfo))
                .Subscribe(x =>
                {
                    _collisionController.DisableAllAttackCollision();
                }).AddTo(this);
            #endregion

        }

        private bool IsComboState(AnimatorStateInfo stateInfo)
        {
            if (stateInfo.IsName("Combo1")) return true;
            if (stateInfo.IsName("Combo2")) return true;
            if (stateInfo.IsName("Combo3")) return true;

            return false;
        }

        private bool IsJumpingState(ActionState value)
        {
            return value == ActionState.Jump || value == ActionState.JumpAttack;
        }

        public void CameraShake(CameraShakeEventData.CameraShakeType type)
        {
            _cameraShakeEventPublisher.Publish(new CameraShakeEventData(type));
        }

    }
}