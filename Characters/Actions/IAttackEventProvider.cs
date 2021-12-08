using System;
using UniRx;

namespace Characters.Actions
{
        public interface IAttackEventProvider 
        {
                public IObservable<AttackType> OnAttack { get; }
                public IObservable<Unit> OnChargeStart { get; }
                public IObservable<Unit> OnChargeEnd { get; }
                public IObservable<Unit> OnBlockCancel { get; }
        }
}
