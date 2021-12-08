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
    /// <summary>
    /// 目的地に向かってまっすぐ移動
    /// </summary>
    [TaskCategory("EnemyActions")]
    public class MoveToTargetSimple : Action
    {
        public SharedTransform targetTransform;
        public SharedVector3 targetPosition;
        public float timeout;

        private EnemyCommandEventProvider _enemyCommandEventProvider;
        private ICharacterStateProvider _characterStateProvider;
        

        private IDisposable timer;
        private bool timerUp;
	
        public override void OnStart()
        {
            _enemyCommandEventProvider = GetComponent<EnemyCommandEventProvider>();
            _characterStateProvider = GetComponent<CharacterCore>();

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
            if (targetTransform.Value != null)
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
            
            // 目的地まで移動したら終了
            if (targetTransform.Value != null)
            {
                if (Vector3.Distance(transform.position, targetPosition.Value) < 0.5f)
                {
                    timer?.Dispose();
                    _enemyCommandEventProvider.Move(Vector3.zero);
                    return TaskStatus.Success;
                }
            }
            
            var dir = (targetPosition.Value - transform.position).normalized;
            
            _enemyCommandEventProvider.Move(dir);
            return TaskStatus.Running;

        }
        

    }
}