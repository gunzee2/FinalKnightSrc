using UniRx;

namespace Characters
{
    public interface IHealthReadable
    {
        
        public IReadOnlyReactiveProperty<float> HealthRatio { get; }
    }
}
