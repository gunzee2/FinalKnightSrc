using System;
using Characters.Damages;
using Chronos;
using UniRx;
using UnityEngine;

namespace Characters
{
    public class CharacterTimelineController : MonoBehaviour
    {
        private ICharacterEventProvider _characterEventProvider;
        private ICharacterCollisionEventProvider _collisionEventProvider;
        LocalClock _localClock;

        [SerializeField] private float attackerHitStopTimeScale = 0.05f;
        [SerializeField] private float receiverHitStopTimeScale = 0.05f;

        [SerializeField] private int attackerHitStopFrame = 5;
        [SerializeField] private int receiverHitStopFrame = 5;


        private void Awake()
        {
            _characterEventProvider = GetComponent<ICharacterEventProvider>();
            _collisionEventProvider = GetComponent<ICharacterCollisionEventProvider>();
            _localClock = GetComponent<LocalClock>();
        }

        // Start is called before the first frame update
        void Start()
        {
            #region ヒットストップ

            // 攻撃を与える側のヒットストップ
            IDisposable hitStopDisposable = null;
            _collisionEventProvider.OnAttackCollisionHit.Subscribe(x =>
                {
                    if (attackerHitStopFrame <= 0) return;
                    _localClock.localTimeScale = attackerHitStopTimeScale;

                    Debug.Log("attacker hit stop start", gameObject);
                    hitStopDisposable?.Dispose();
                    hitStopDisposable = Observable.TimerFrame(attackerHitStopFrame).Subscribe(_ =>
                        {
                            Debug.Log("attacker hit stop end", gameObject);
                            _localClock.localTimeScale = 1f;
                        }
                    ).AddTo(this);
                }
            ).AddTo(this);


            IDisposable hitStopDisposable2 = null;
            _characterEventProvider.OnDamaged.Subscribe(x =>
                {
                    // ノックバックがある場合はヒットストップしない.
                    if (x.knockBack != KnockBack.None) return;
                    if (receiverHitStopFrame <= 0) return;

                    _localClock.localTimeScale = receiverHitStopTimeScale;

                    Debug.Log("receiver hit stop start", gameObject);
                    //Time.timeScale = 0.025f;
                    hitStopDisposable2?.Dispose();
                    hitStopDisposable2 = Observable.TimerFrame(receiverHitStopFrame).Subscribe(_ =>
                        {
                            Debug.Log("receiver hit stop end", gameObject);
                            _localClock.localTimeScale = 1f;
                            //Time.timeScale = 1f;
                        }
                    ).AddTo(this);
                }
            ).AddTo(this);

            #endregion
        }
    }
}