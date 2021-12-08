using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Characters.Inputs;
using Pathfinding;
using UniRx;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace Characters.Actions.Behaviour.Actions
{
    [TaskCategory("EnemyActions")]
    public class MoveToTargetAStar : Action
    {
        public bool isSuspendNearTarget = false;
        public SharedTransform targetTransform;
        public SharedVector3 targetPosition;
        public float timeout;

        private EnemyCommandEventProvider _enemyCommandEventProvider;
        private ICharacterStateProvider _characterStateProvider;
        
        private Seeker _seeker;
        private Path _path;
        private int _currentWayPoint = 0;

        private IDisposable timer;
        private bool timerUp;
	
        public override void OnStart()
        {
            _enemyCommandEventProvider = GetComponent<EnemyCommandEventProvider>();
            _characterStateProvider = GetComponent<CharacterCore>();
            _seeker = GetComponent<Seeker>();

            _seeker.StartPath(transform.position,targetPosition.Value, (p) =>
            {
                if (p.error) return;
                
                _path = p;
                _currentWayPoint = 0;
            });

            // タイムアウト設定.
            timer?.Dispose();
            timerUp = false;
            timer = Observable.Timer(TimeSpan.FromSeconds(timeout)).Subscribe(_ =>
            {
                timerUp = true;
            }).AddTo(gameObject);
        }
        

        public override TaskStatus OnUpdate()
        {
            // プレーヤーとの距離が十分に近づいたら移動を中断
            if (isSuspendNearTarget && targetTransform.Value != null)
            {
                if (Vector3.Distance(transform.position, targetTransform.Value.position) < 1f)
                {
                    timer?.Dispose();
                    _enemyCommandEventProvider.Move(Vector3.zero);
                    return TaskStatus.Success;
                }
            }
            
            // キャラクターの状態が移動中でもアイドル状態でもなければ移動を中断する.
            if (_characterStateProvider.CurrentActionState.Value != ActionState.Move &&
                _characterStateProvider.CurrentActionState.Value != ActionState.Idle)
            {
                Debug.Log("terminated ", gameObject);
                timer?.Dispose();
                _enemyCommandEventProvider.Move(Vector3.zero);
                return TaskStatus.Failure;
            }
            
            // タイムアウトしたら処理を中断する
            if (timerUp)
            {
                Debug.Log("timeout", gameObject);
                timer?.Dispose();
                _enemyCommandEventProvider.Move(Vector3.zero);
                return TaskStatus.Failure;
            }
            
            // パスを生成できなかったら継続(パス計算がまだ終わってない可能性がある)
            if (_path == null)
            {
                Debug.Log("path not set", gameObject);
                return TaskStatus.Running;
            }

            // パスを完走したらSuccess
            if (_currentWayPoint >= _path.vectorPath.Count)
            {
                Debug.Log("waypoint completed", gameObject);
                timer?.Dispose();
                _enemyCommandEventProvider.Move(Vector3.zero);
                return TaskStatus.Success;
            }

            var nextPoint = _path.vectorPath[_currentWayPoint];
            nextPoint.y = 0;
            var currentPoint = transform.position;
            currentPoint.y = 0;
            
            var dir = (nextPoint - currentPoint).normalized;
            if (Vector3.Distance (transform.position,_path.vectorPath[_currentWayPoint]) < 0.5f) {
                _currentWayPoint++;
            }
            
            _enemyCommandEventProvider.Move(dir);
            return TaskStatus.Running;

        }
        

    }
}