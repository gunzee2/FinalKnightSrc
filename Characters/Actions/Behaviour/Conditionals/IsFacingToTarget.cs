using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Characters.Actions.Behaviour.Conditionals
{
	[TaskCategory("EnemyConditonals")]
	public class IsFacingToTarget : Conditional
	{
        public SharedTransform targetTransform;
		
		public override TaskStatus OnUpdate()
		{
			// ターゲットより左に居て右を向いていればSuccess
			if (transform.position.x < targetTransform.Value.position.x)
			{
				if ((transform.forward.normalized - Vector3.right).sqrMagnitude < 0.01f) return TaskStatus.Success;
				
				return TaskStatus.Failure;

			}

			// ターゲットより右に居て左を向いていればSuccess
			if ((transform.forward.normalized - Vector3.left).sqrMagnitude < 0.01f) return TaskStatus.Success;
				
			return TaskStatus.Failure;
		}
	}
}