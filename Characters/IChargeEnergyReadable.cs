using UniRx;

namespace Characters
{
    public interface IChargeEnergyReadable
    {
        
        public IReadOnlyReactiveProperty<float> ChargeRatio { get; }
    }
}
