using BehaviorDesigner.Runtime.Tasks;
using Characters.Inputs;
using UnityEngine;

namespace Characters.Actions.Behaviour.Actions
{
	[TaskCategory("EnemyActions")]
	public class UnderAttack : Action
	{
        private EnemyCommandEventProvider _enemyCommandEventProvider;
		public override void OnStart()
		{
            _enemyCommandEventProvider = GetComponent<EnemyCommandEventProvider>();
		}

		public override TaskStatus OnUpdate()
		{
			
			_enemyCommandEventProvider.Attack(InputCommand.DownAttack);
			_enemyCommandEventProvider.AttackRelease();
			return TaskStatus.Success;
		}
	}
}