using System;
using UniRx;
using UnityEngine;

namespace Characters.Actions
{
    public interface IMoveEventProvider
    { 
        public IReadOnlyReactiveProperty<bool> IsMoving { get; }
        public IReadOnlyReactiveProperty<bool> IsAnalogueMoving { get; }
        public IObservable<Vector3> OnRotate { get; }
        public bool IsStepMove { get; }
    }
}
