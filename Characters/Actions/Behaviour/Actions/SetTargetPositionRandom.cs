using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using Random = UnityEngine.Random;

namespace Characters.Actions.Behaviour.Actions
{
	[TaskCategory("EnemyActions")]
	public class SetTargetPositionRandom : Action
	{
		public float range;
		public SharedVector3 targetPosition;
		public override void OnStart()
		{
		
		}

		public override TaskStatus OnUpdate()
		{
			var newPos = targetPosition.Value;
			newPos.x += Random.Range(-range, range);
			newPos.z +=  Random.Range(-range, range);

			targetPosition.Value = newPos;
			
			return TaskStatus.Success;
		}
	}
}