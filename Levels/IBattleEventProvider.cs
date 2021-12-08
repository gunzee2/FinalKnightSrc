using System;
using UniRx;

namespace Levels
{
    public interface IBattleEventProvider 
    {
        public IObservable<Unit> OnAllEnemyDead { get; }
    }
}