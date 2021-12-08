using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Characters.Inputs;
using UnityEngine;

namespace Characters.Actions.Behaviour.Actions
{
    [TaskCategory("EnemyActions")]
    public class RotateCharacter : Action
    {
        private EnemyCommandEventProvider _enemyCommandEventProvider;
        
        public override void OnStart()
        {
            _enemyCommandEventProvider = GetComponent<EnemyCommandEventProvider>();
        }

        public override TaskStatus OnUpdate()
        {
            _enemyCommandEventProvider.Rotate();
            return TaskStatus.Success;
        }
    }
}