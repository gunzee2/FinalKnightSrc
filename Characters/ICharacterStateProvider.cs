using Characters.Actions;
using UniRx;

namespace Characters
{
    public interface ICharacterStateProvider
    {
    
        IReadOnlyReactiveProperty<ActionState> CurrentActionState { get; }
        IReadOnlyReactiveProperty<bool> IsGrounded { get; }
        IReadOnlyReactiveProperty<bool> IsInvincible { get; }
        IReadOnlyReactiveProperty<bool> HaveSuperArmor{ get; }
        void ChangeCurrentActionState(ActionState state);
    }
}