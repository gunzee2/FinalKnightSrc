using System;
using BehaviorDesigner.Runtime;
using UniRx;
using UnityEngine;

namespace Characters
{
    public class EnemyBehaviorController : MonoBehaviour
    {
        [SerializeField] private BehaviorTree behaviorTree;
        [SerializeField] private float detectLengthX;

        private Transform _targetTransform;
        private bool _isDetected = false;
        private IDisposable _disposable;

        // Start is called before the first frame update
        void Start()
        {
            _targetTransform = GameObject.FindGameObjectWithTag("Player").transform;

            _disposable = this.ObserveEveryValueChanged(_ => _targetTransform.position.x)
                .Where(_ => !_isDetected)
                .Where(_ => Mathf.Abs(_targetTransform.position.x - transform.position.x) <= detectLengthX)
                .Subscribe(_ =>
                    {
                        Debug.Log("enable behavior");
                        behaviorTree.EnableBehavior();
                        _isDetected = true;
                        _disposable?.Dispose();
                    }
                ).AddTo(this);
        }

        public void EnableBehavior()
        {
            Debug.Log("enable behavior Manual");
            _disposable?.Dispose();
            behaviorTree.EnableBehavior();
            _isDetected = true;
        }
    }
}