using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Characters.Inputs;
using UniRx;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace Characters.Actions.Behaviour.Actions
{
    [TaskCategory("EnemyActions")]
    public class MoveAttack : Action
    {
        
        public SharedVector3 targetPosition;
        public float timeout;
        private IDisposable timer;
        private bool timerUp;
        
        private EnemyCommandEventProvider _enemyCommandEventProvider;
        private ICharacterStateProvider _characterStateProvider;
	
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
            
			_enemyCommandEventProvider.Attack(InputCommand.MoveAttack);
        }

        public override TaskStatus OnUpdate()
        {
            if (Vector3.Distance(transform.position, targetPosition.Value) < 0.25f)
            {
                _enemyCommandEventProvider.AnalogueMove(Vector3.zero);
                return TaskStatus.Success;
            }
            
            // キャラクターの状態が移動中でもアイドル状態でもなければ移動を中断する.
            if (_characterStateProvider.CurrentActionState.Value != ActionState.Move &&
                _characterStateProvider.CurrentActionState.Value != ActionState.Idle && 
                _characterStateProvider.CurrentActionState.Value != ActionState.MoveAttack
                )
            {
                return TaskStatus.Failure;
            }
            
            // タイムアウトしたら処理を中断する
            if (timerUp)
            {
                _enemyCommandEventProvider.AnalogueMove(Vector3.zero);
                return TaskStatus.Failure;
            }
            

            var direction = targetPosition.Value - transform.position;
            _enemyCommandEventProvider.AnalogueMove(direction.normalized);
            
			return TaskStatus.Running;
        }
    }
}