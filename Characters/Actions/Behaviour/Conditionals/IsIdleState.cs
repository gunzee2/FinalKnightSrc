using BehaviorDesigner.Runtime.Tasks;

namespace Characters.Actions.Behaviour.Conditionals
{
	[TaskCategory("EnemyConditonals")]
	public class IsIdleState : Conditional
	{
		private ICharacterStateProvider _characterStateProvider;
		public override void OnStart()
		{
			_characterStateProvider  = GetComponent<CharacterCore>();
		}

		public override TaskStatus OnUpdate()
		{
			return _characterStateProvider.CurrentActionState.Value == ActionState.Idle ? TaskStatus.Success : TaskStatus.Failure;
		}
	}
}