using Characters.Actions;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
    public interface ICharacterInformationProvider
    {
        public string Name { get; }
        public Sprite PortraitImage { get; }
        public Sprite PortraitDamagedImage { get; }
        public CharacterStatus InitialStatus { get; }
        public CharacterType CharacterType { get; }
        public IReadOnlyReactiveProperty<float> HealthRatio { get; }
        public IReadOnlyReactiveProperty<float> ChargeRatio { get; }

        public IReadOnlyReactiveProperty<int> CurrentHealth { get; }
    }
}