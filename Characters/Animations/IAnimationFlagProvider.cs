using UniRx;

namespace Characters.Animations
{
        public interface IAnimationFlagProvider
        {
                public IReadOnlyReactiveProperty<bool> IsAttackAnimationPlaying { get; }
        }
}
