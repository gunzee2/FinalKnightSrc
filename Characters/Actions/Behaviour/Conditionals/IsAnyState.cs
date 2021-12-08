using BehaviorDesigner.Runtime.Tasks;

namespace Characters.Actions.Behaviour.Conditionals
{
	[TaskCategory("EnemyConditonals")]
	public class IsAnyState : Conditional
	{
		public ActionState state;
		private ICharacterStateProvider _characterStateProvider;
		public override void OnStart()
		{
			_characterStateProvider  = GetComponent<CharacterCore>();
		}

		public override TaskStatus OnUpdate()
		{
			return _characterStateProvider.CurrentActionState.Value == state ? TaskStatus.Success : TaskStatus.Failure;
		}
	}
}