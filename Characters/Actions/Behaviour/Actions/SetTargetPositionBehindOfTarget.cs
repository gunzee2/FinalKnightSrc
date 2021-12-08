using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Characters.Actions.Behaviour.Actions
{
	[TaskCategory("EnemyActions")]
	public class SetTargetPositionBehindOfTarget : Action
	{
		public SharedTransform targetTransform;
		public SharedVector3 targetPosition;

		public float offsetX;
		
		public override void OnStart()
		{
		}

		public override TaskStatus OnUpdate()
		{
			var targetPos = targetTransform.Value.position;
			
            if (transform.position.x < targetPos.x)
            {
                targetPos.x += offsetX;
            }
            else
            {
                targetPos.x -= offsetX;
            }

            targetPosition.Value = targetPos;
            
            return TaskStatus.Success;
		}
	}
}