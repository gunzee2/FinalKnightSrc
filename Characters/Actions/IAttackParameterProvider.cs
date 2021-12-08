using System;
using UniRx;

namespace Characters.Actions
{
    public interface IAttackParameterProvider
    {
        public Guid DamageId { get; }
        public IReadOnlyReactiveProperty<float> ChargeRatio { get; }
    }
}
