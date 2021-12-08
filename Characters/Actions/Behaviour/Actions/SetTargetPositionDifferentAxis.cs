using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Characters.Actions.Behaviour.Actions
{
	[TaskCategory("EnemyActions")]
	public class SetTargetPositionDifferentAxis: Action
	{
		public SharedTransform targetTransform;
		public SharedVector3 targetPosition;

		public float offsetZ;
		
		public override void OnStart()
		{
		}

		public override TaskStatus OnUpdate()
		{
			var targetPos = transform.position;
			
			// ターゲットが自分より下にいる場合、更に上に離れる
            if (transform.position.z >= targetTransform.Value.position.z)
            {
	            targetPos.z += offsetZ;
            }
            else
            {
                targetPos.z -= offsetZ;
            }

            targetPosition.Value = targetPos;
            
            return TaskStatus.Success;
		}
	}
}