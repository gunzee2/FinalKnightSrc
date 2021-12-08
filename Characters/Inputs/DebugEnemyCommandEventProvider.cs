using System;
using UniRx;
using UnityEngine;

namespace Characters.Inputs
{
    public class DebugEnemyCommandEventProvider : MonoBehaviour,ICommandEventProvider
    {
        public IObservable<InputCommand> OnCommandInput => _onCommandInput;
        public IReadOnlyReactiveProperty<Vector3> AnalogueMovementInput => _analogueMovementInput;
        public Vector3ReactiveProperty _analogueMovementInput = new Vector3ReactiveProperty();
        
        private readonly Subject<InputCommand> _onCommandInput = new Subject<InputCommand>();
    
        // Start is called before the first frame update
        void Start()
        {
            Observable.Interval(TimeSpan.FromSeconds(1f)).Subscribe(_ =>
            {
                _onCommandInput.OnNext(InputCommand.Attack);
                _onCommandInput.OnNext(InputCommand.AttackRelease);
            }).AddTo(this);
        }
    }
}