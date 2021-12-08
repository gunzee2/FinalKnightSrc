using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Characters.Actions.Behaviour.Conditionals
{
	[TaskCategory("EnemyConditonals")]
    public class FindPlayer : Conditional
    {
        public SharedTransform targetTransform;
	
        public override TaskStatus OnUpdate()
        {
            var target = GameObject.FindGameObjectWithTag("Player").transform;

            targetTransform.Value = target;
        
            return target ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}