using UniRx;

namespace Characters
{
    public interface ICharacterDataProvider
    {
        CharacterStatus InitialStatus { get; }
        IReadOnlyReactiveProperty<int> CurrentHealth { get; }
        IReadOnlyReactiveProperty<bool> IsInvincible { get; }
        
    }
}
