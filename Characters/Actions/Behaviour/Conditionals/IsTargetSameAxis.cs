using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Characters.Actions.Behaviour.Conditionals
{
	[TaskCategory("EnemyConditonals")]
	public class IsTargetSameAxis : Conditional
	{
		public SharedTransform targetTransform;
		public float range;
	
		public override TaskStatus OnUpdate()
		{
			var diff = transform.position.z - targetTransform.Value.position.z;
			
			return Mathf.Abs(diff) <= range ? TaskStatus.Success : TaskStatus.Failure;
		}
	}
}