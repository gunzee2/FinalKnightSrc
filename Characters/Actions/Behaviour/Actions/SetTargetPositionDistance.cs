using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Characters.Actions.Behaviour.Actions
{
	[TaskCategory("EnemyActions")]
	public class SetTargetPositionDistance : Action
	{
		public SharedTransform targetTransform;
		public SharedVector3 targetPosition;

		public float distance;
		
		public override void OnStart()
		{
		}

		public override TaskStatus OnUpdate()
		{
			var targetPos = transform.position;
			// 対象より左にいる場合、左に移動
			// 対象より右にいる場合、右に移動
			if (transform.position.x < targetTransform.Value.position.x)
				targetPos.x -= distance;
			else
				targetPos.x += distance;
			
            targetPosition.Value = targetPos;
            
            return TaskStatus.Success;
		}
    
	}
}