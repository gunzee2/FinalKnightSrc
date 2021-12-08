using System;
using System.Collections.Generic;
using UniRx;

namespace Characters
{
        public interface ICharacterCollisionEventProvider
        {
                public List<IObservable<IHitCollisionDataProvider>> OnAttackComboCollisionHit { get; }
                public IObservable<IHitCollisionDataProvider> OnJumpAttackCollisionHit1 { get; }
                public IObservable<IHitCollisionDataProvider> OnJumpAttackCollisionHit2 { get; }
                public IObservable<IHitCollisionDataProvider> OnChargeAttackCollisionHit { get; }
                public IObservable<IHitCollisionDataProvider> OnMegaCrashCollisionHit { get; }
                public IObservable<IHitCollisionDataProvider> OnThrowItemCollisionHit { get; }
                public IObservable<Unit> OnThrowItemCollisionDrop { get; }
                public IObservable<IHitCollisionDataProvider> OnAttackCollisionHit { get; }
        }
}
