using System;
using System.Linq;
using System.Numerics;
using Characters.Animations;
using Characters.Damages;
using Characters.Inputs;
using Chronos;
using DG.Tweening.Core;
using MoreMountains.NiceVibrations;
using Pathfinding.Util;
using Sirenix.OdinInspector;
using UniRx;
using UniRx.Diagnostics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace Characters.Actions
{
    public class CharacterMover : SerializedMonoBehaviour, IMoveEventProvider
    {
        public bool IsStepMove => isStepMove;
        [SerializeField] private bool isStepMove;
        [SerializeField] private float stepMoveCooldownTime;
        

        private Rigidbody _rb;
        private bool _isPlayer;
        private Timeline _timeline;

        private ICharacterInformationProvider _characterInformationProvider;
        private ICharacterStateProvider _characterStateProvider;
        private ICharacterEventProvider _characterEventProvider;
        
        private ICommandEventProvider _commandEventProvider;
        private IAnimationFlagProvider _animationFlagProvider;
        
        public IReadOnlyReactiveProperty<bool> IsMoving => _isMoving;
        private BoolReactiveProperty _isMoving = new BoolReactiveProperty();
        
        public IReadOnlyReactiveProperty<bool> IsAnalogueMoving => _isAnalogueMoving;
        private BoolReactiveProperty _isAnalogueMoving = new BoolReactiveProperty();
        
        
        public IObservable<Vector3> OnRotate => _onRotate;
        private readonly Subject<Vector3> _onRotate = new Subject<Vector3>();
        
        private const float ROTATION_Y = -90f;

        private bool isMoveCooldownUp = true;

        private void Awake()
        {
            _animationFlagProvider = GetComponent<IAnimationFlagProvider>();
            _commandEventProvider = GetComponent<ICommandEventProvider>();
            _isPlayer = GetComponent<CharacterCore>().CharacterType == CharacterType.Player;

            _rb = GetComponent<Rigidbody>();
            _timeline = GetComponent<Timeline>();

            _characterInformationProvider = GetComponent<ICharacterInformationProvider>();
            _characterStateProvider= GetComponent<ICharacterStateProvider>();
            _characterEventProvider = GetComponent<ICharacterEventProvider>();
        }

        private void Start()
        {
            
            #region ??????
            // ??????.
            _commandEventProvider
                .OnCommandInput
                //.Do(x => Debug.Log($"command: {x}", gameObject))
                .BatchFrame(0, FrameCountType.Update)
                .Where(x => !x.Contains(InputCommand.Attack))
                .Where(x => !x.Contains(InputCommand.AttackRelease))
                .Where(x => !x.Contains(InputCommand.Jump))
                .Where(x => x.Any(IsDirectionInput))
                .Select(x => x.First(IsDirectionInput))
                .Where(_ => CanMove())
                .Subscribe(x =>
                {
                    var vec = MoveDirectionToVector3(x);

                    if (IsStepMove)
                    {
                        if (!isMoveCooldownUp) return;
                        _isMoving.SetValueAndForceNotify(vec.magnitude > 0.1f);
                        StepMove(vec);
                    }
                    else
                    {
                        _isMoving.SetValueAndForceNotify(vec.magnitude > 0.1f);
                        Move(vec);
                    }
                
                    //_characterStateProvider.ChangeCurrentActionState(_isMoving.Value ? ActionState.Move : ActionState.Idle);
                    if(_isMoving.Value) _characterStateProvider.ChangeCurrentActionState(ActionState.Move); 
                }).AddTo(this);

            _commandEventProvider
                .AnalogueMovementInput
                .Where(_ => CanMove())
                .Subscribe(x =>
                {
                    //Debug.Log($"analogue move {x}", gameObject);
                    _isAnalogueMoving.Value = x.magnitude > 0.1f;
                    Run(x);

                    if (_isAnalogueMoving.Value == false)
                    {
                        _characterStateProvider.ChangeCurrentActionState(ActionState.Idle);
                    }
                }).AddTo(this);

            
            #endregion

            #region ????????????
            // ????????????
            _commandEventProvider
                .OnCommandInput
                .Where(_ => _isPlayer)
                .Where(x => x != InputCommand.Neutral)
                .Where(IsDirectionInput)
                .Where(_ => _characterStateProvider.CurrentActionState.Value == ActionState.Idle ||
                            _characterStateProvider.CurrentActionState.Value == ActionState.Move || 
                            _characterStateProvider.CurrentActionState.Value == ActionState.BlockCancel
                )
                .Select(MoveDirectionToVector3)
                .Where(x => Math.Abs(Mathf.Sign(x.x) - Mathf.Sign(transform.forward.x)) >= 0.1f)
                .Subscribe(x =>
                {
                    Rotate(x);
                }).AddTo(this);

            // ??????????????????(??????????????????????????????????????????????????????????????????????????????????????????????????????????????????)
            _commandEventProvider
                .OnCommandInput
                .Where(_ => !_isPlayer)
                .Where(x => x == InputCommand.EnemyRotate)
                .Select(_ => -transform.forward)
                .Subscribe(x =>
                {
                    Rotate(x);
                }).AddTo(this);
            #endregion
            
            #region ????????????
            _commandEventProvider.OnCommandInput
                .BatchFrame(0, FrameCountType.Update)
                .Where(x => x.Contains(InputCommand.Jump))
                .Where(x => !x.Contains(InputCommand.Attack))
                .Where(x => !x.Contains(InputCommand.MegaCrash))
                .ThrottleFirstFrame(10)
                .Where(_ => CanJump())
                .Subscribe(x => // ???????????????????????????????????????????????????????????????????????????????????????????????????.
                {
                    // ??????????????????????????????????????????????????????????????????.
                    if (x.Any(y => IsDirectionInput(y) && y != InputCommand.Neutral))
                    {
                        // ????????????????????????????????????.
                        var vec = MoveDirectionToVector3(x.FirstOrDefault(y => IsDirectionInput(y) && y != InputCommand.Neutral));
                        Rotate(vec);
                        Move(vec);
                    }
                        
                    _characterStateProvider.ChangeCurrentActionState(ActionState.Jump);
                    Jump();
                    
                }).AddTo(this);
            #endregion

            #region ??????????????????????????????(?????????????????????????????????)
            // ???????????????????????????????????????????????????(?????????????????????????????????).
            _characterStateProvider.CurrentActionState.Where(x => x == ActionState.GroundAttack || x == ActionState.MegaCrash ||  x == ActionState.ThrowItem).Subscribe(_ =>
            {
                StopMove();
            }).AddTo(this);
            // ?????????????????????????????????????????????????????????(?????????????????????????????????).
            _characterStateProvider.CurrentActionState.Where(x => x == ActionState.Damage).Subscribe(_ =>
            {
                Debug.Log($"Damaged Move Stop", this.gameObject);
                StopMove();
            }).AddTo(this);
            // ???????????????????????????????????????????????????????????????(?????????????????????????????????).
            _characterStateProvider.CurrentActionState.Where(x => x == ActionState.Pickup).Subscribe(_ =>
            {
                StopMove();
            }).AddTo(this);

            _characterStateProvider.CurrentActionState.Where(x => x == ActionState.Repelled).Subscribe(_ => 
                {
                    StopMove();
                })
                .AddTo(this);
            #endregion

            #region ??????????????????.
            _characterEventProvider
                .OnKnockBack
                .Subscribe(KnockBack).AddTo(this);
            
            #endregion

        }

        /// <summary>
        /// Animation????????????????????????????????????
        /// </summary>
        public void SlashMove()
        {
            if (!CanMove()) return;
            
            Dash(transform.forward, 12f);
        }

        private void StopMove()
        {
            _isMoving.Value = false;
            _isAnalogueMoving.Value = false;

            if (_characterStateProvider.IsGrounded.Value) _rb.velocity = Vector3.zero;
        }

        private void KnockBack(CharacterKnockbackData knockBackData)
        {
            // ??????????????????????????????????????????????????????????????????.
            if (knockBackData.KnockBackDirection == Damages.KnockBack.None) return;
            

            var vec = Vector3.zero;
            
            if (knockBackData.KnockBackDirection == Damages.KnockBack.DistanceAway)
            {
                if (transform.position.x < knockBackData.AttackerPosition.x)
                {
                    // ???????????????
                    Rotate(Vector3.right);
                    // ????????????
                    vec = Vector3.left;
                }
                else
                {
                    // ???????????????
                    Rotate(Vector3.left);
                    // ????????????
                    vec = Vector3.right;
                }
            }
            else
            {
                // ???????????????????????????????????????
                if (IsFacingRight(knockBackData.AttackerFacingDirection))
                {
                    // ??????????????????????????????????????????
                    if (knockBackData.KnockBackDirection == Damages.KnockBack.Forward)
                    {
                        // ???????????????
                        Rotate(Vector3.left);
                        // ????????????
                        vec = Vector3.right;
                    }
                    // ??????????????????????????????????????????
                    else if (knockBackData.KnockBackDirection == Damages.KnockBack.Reverse)
                    {
                        // ???????????????
                        Rotate(Vector3.right);
                        // ????????????
                        vec = Vector3.left;
                    }
                }
                else
                {
                    // ??????????????????????????????????????????
                    if (knockBackData.KnockBackDirection == Damages.KnockBack.Forward)
                    {
                        // ???????????????
                        Rotate(Vector3.right);
                        // ????????????
                        vec = Vector3.left;
                    }
                    // ??????????????????????????????????????????
                    else
                    {
                        // ???????????????
                        Rotate(Vector3.left);
                        // ????????????
                        vec = Vector3.right;
                    }
                }
            }
            
            Debug.Log($"knockback Vector => {vec}, {knockBackData.KnockBackDirection}, {knockBackData.IsBlowAway}", gameObject);

            vec *= knockBackData.SlidePower;

            // ????????????????????????ON????????????????????????
            if (knockBackData.IsBlowAway) vec += Vector3.up * knockBackData.UpPower;

            _timeline.rigidbody.velocity = Vector3.zero;
            _timeline.rigidbody.velocity = vec;
        }

        private bool IsFacingRight(Vector3 direction)
        {
            return direction.normalized == Vector3.right;
        }


        private bool IsDirectionInput(InputCommand command)
        {
            return command >= InputCommand.DownLeft && command <= InputCommand.UpRight;
        }

        private static Vector3 MoveDirectionToVector3(InputCommand x)
        {
            var vec = x switch
            {
                InputCommand.DownLeft => new Vector3(-1, 0, -1),
                InputCommand.Down => new Vector3(0, 0, -1),
                InputCommand.DownRight => new Vector3(1, 0, -1),
                InputCommand.Left => new Vector3(-1, 0, 0),
                InputCommand.Neutral => new Vector3(0, 0, 0),
                InputCommand.Right => new Vector3(1, 0, 0),
                InputCommand.UpLeft => new Vector3(-1, 0, 1),
                InputCommand.Up => new Vector3(0, 0, 1),
                InputCommand.UpRight => new Vector3(1, 0, 1),
                _ => new Vector3(0, 0, 0)
            };
            return vec;
        }

        private void Jump()
        {
            _rb.AddForce(Vector3.up * _characterInformationProvider.InitialStatus.JumpPower, ForceMode.Impulse);
            Debug.Log($"jump velocity:{_rb.velocity}");
        }

        private void Move(Vector3 direction)
        {
            _rb.velocity = new Vector3(direction.x * _characterInformationProvider.InitialStatus.MoveSpeedX, _rb.velocity.y, direction.z * _characterInformationProvider.InitialStatus.MoveSpeedZ);
        }

        private void StepMove(Vector3 direction)
        {
            _rb.AddForce(new Vector3(direction.x * _characterInformationProvider.InitialStatus.DashSpeedMax, 0, direction.z * _characterInformationProvider.InitialStatus.DashSpeedMax), ForceMode.Impulse);
            isMoveCooldownUp = false;
            Observable.Timer(TimeSpan.FromSeconds(stepMoveCooldownTime)).Subscribe(_ =>
            {
                isMoveCooldownUp = true;
                StopMove();
            }).AddTo(this);
        }

        private void Dash(Vector3 direction, float power)
        {
            _rb.AddForce(new Vector3(direction.x * power, 0, direction.z * power), ForceMode.Impulse);
        }

        private void Run(Vector3 direction)
        {
            var speed = Random.Range(_characterInformationProvider.InitialStatus.DashSpeedMin,
                _characterInformationProvider.InitialStatus.DashSpeedMax);
            _rb.velocity = new Vector3(direction.x * speed, _rb.velocity.y, direction.z * speed);
        }

        private void Rotate(Vector3 direction)
        {
            if (direction.x > 0)
            {
                transform.rotation = Quaternion.Euler(0, Mathf.Abs(ROTATION_Y), 0);
                _onRotate.OnNext(transform.forward);
            }
            else if (direction.x < 0)
            {
                transform.rotation = Quaternion.Euler(0, ROTATION_Y, 0);
                _onRotate.OnNext(transform.forward);
            }
        }

        private bool CanMove()
        {
            if (!_characterStateProvider.IsGrounded.Value) return false;
            if (_characterStateProvider.CurrentActionState.Value != ActionState.Idle &&
                _characterStateProvider.CurrentActionState.Value != ActionState.Move &&
                _characterStateProvider.CurrentActionState.Value != ActionState.MoveAttack) return false;

            return true;
        }
        

        private bool CanRotate()
        {
            return !_animationFlagProvider.IsAttackAnimationPlaying.Value;
        }

        private bool CanJump()
        {
            // ??????????????????Idle???Move??????????????????????????????????????????.
            if (!_characterStateProvider.IsGrounded.Value) return false;
            if (_characterStateProvider.CurrentActionState.Value != ActionState.Idle && _characterStateProvider.CurrentActionState.Value != ActionState.Move) return false;
            
            return true;
        }
        
        
        
    }
}