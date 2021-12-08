using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Characters.Actions.Behaviour.Conditionals
{
	[TaskCategory("EnemyConditonals")]
	public class IsTargetOutOfRange : Conditional
	{
		public SharedTransform targetTransform;
		public SharedFloat range;
	
		public override TaskStatus OnUpdate()
		{
			var diff = transform.position.x - targetTransform.Value.position.x;
			
			return Mathf.Abs(diff) >= range.Value ? TaskStatus.Success : TaskStatus.Failure;
		}
	}
}