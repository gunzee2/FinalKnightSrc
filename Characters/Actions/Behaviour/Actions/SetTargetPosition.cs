using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Characters.Actions.Behaviour.Actions
{
	[TaskCategory("EnemyActions")]
	public class SetTargetPosition : Action
	{
		public SharedTransform targetTransform;
		public SharedVector3 targetPosition;

		public float offset;
		
		public override void OnStart()
		{
		}

		public override TaskStatus OnUpdate()
		{
			var targetPos = targetTransform.Value.position;

			var vec = targetPos - transform.position;

			targetPos += vec.normalized * offset ;

            targetPosition.Value = targetPos;
            
            return TaskStatus.Success;
		}
    
	}
}