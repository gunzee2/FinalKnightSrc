using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Characters.Inputs;

namespace Characters.Actions.Behaviour.Actions
{
	[TaskCategory("EnemyActions")]
	public class SimpleMoveInput : Action
	{
		public SharedTransform targetTransform;
		
        private EnemyCommandEventProvider _enemyCommandEventProvider;
		public override void OnStart()
		{
            _enemyCommandEventProvider = GetComponent<EnemyCommandEventProvider>();
		}

		public override TaskStatus OnUpdate()
		{
			
			_enemyCommandEventProvider.Move((targetTransform.Value.position - transform.position).normalized);
			return TaskStatus.Success;
		}
	}
}