using BehaviorDesigner.Runtime.Tasks;
using Characters.Inputs;

namespace Characters.Actions.Behaviour.Actions
{
	[TaskCategory("EnemyActions")]
	public class NormalAttack : Action
	{
        private EnemyCommandEventProvider _enemyCommandEventProvider;
		public override void OnStart()
		{
            _enemyCommandEventProvider = GetComponent<EnemyCommandEventProvider>();
		}

		public override TaskStatus OnUpdate()
		{
			
			_enemyCommandEventProvider.Attack(InputCommand.Attack);
			_enemyCommandEventProvider.AttackRelease();
			return TaskStatus.Success;
		}
	}
}