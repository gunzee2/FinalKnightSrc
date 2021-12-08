using UniRx;

namespace Characters.Actions
{
    public interface IBlockEventProvider 
    { 
        public IReadOnlyReactiveProperty<bool> OnBlocking { get; }
    }
}
