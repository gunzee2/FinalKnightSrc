using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Characters.Actions.Behaviour.Conditionals
{
	[TaskCategory("EnemyConditonals")]
    public class IsPlayerAlive : Conditional
    {
        public SharedTransform targetTransform;
	
        public override TaskStatus OnUpdate()
        {
            return targetTransform.Value.GetComponent<CharacterCore>().CurrentHealth.Value > 0 ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}
