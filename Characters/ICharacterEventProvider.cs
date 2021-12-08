using System;
using Characters.Damages;
using UniRx;

namespace Characters
{
    public interface ICharacterEventProvider
    {
        public IObservable<DamageData> OnDamaged { get; }
        public IObservable<CharacterKnockbackData> OnKnockBack { get; }
        public IObservable<CharacterCore> OnBlockSuccessful { get; }
        public IObservable<CharacterCore> OnRepelled { get; }
        public IObservable<Unit> OnFallDownGround { get; }
        public IObservable<ItemController> OnGetPickupItem { get; }
        public IObservable<ItemController> OnThrowPickupItem { get; }
        public IObservable<ItemController> OnLostPickupItem { get; }
    }
}