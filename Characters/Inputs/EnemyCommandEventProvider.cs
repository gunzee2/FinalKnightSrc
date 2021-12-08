using System;
using UniRx;
using UnityEngine;

namespace Characters.Inputs
{
    public class EnemyCommandEventProvider : MonoBehaviour,ICommandEventProvider
    {
        public IObservable<InputCommand> OnCommandInput => _onCommandInput;
        private readonly Subject<InputCommand> _onCommandInput = new Subject<InputCommand>();
        public IReadOnlyReactiveProperty<Vector3> AnalogueMovementInput => _analogueMovementInput;
        public Vector3ReactiveProperty _analogueMovementInput = new Vector3ReactiveProperty();


        public void Move(Vector3 vec)
        {
            var directionCommand = ChangeDirectionVectorToInputCommand(vec);
            
            _onCommandInput.OnNext(directionCommand);
        }

        public void AnalogueMove(Vector3 vec)
        {
            _analogueMovementInput.Value = vec;
        }
        

        public void Rotate()
        {
            _onCommandInput.OnNext(InputCommand.EnemyRotate);
        }

        public void Attack(InputCommand attackCommand)
        {
            _onCommandInput.OnNext(attackCommand);
        }

        public void Jump()
        {
            _onCommandInput.OnNext(InputCommand.Jump);
        }

        public void AttackRelease()
        {
            _onCommandInput.OnNext(InputCommand.AttackRelease);
        }
        

        private InputCommand ChangeDirectionVectorToInputCommand(Vector3 vec)
        {
            if (vec.x <= -0.1f && vec.z <= -0.1f) return InputCommand.DownLeft;
            if (vec.x > -0.1f && vec.x < 0.1f && vec.z <= -0.1f) return InputCommand.Down;
            if (vec.x >= 0.1f && vec.z <= -0.1f) return InputCommand.DownRight;
            
            if (vec.x <= -0.1f && vec.z > -0.1f && vec.z < 0.1f) return InputCommand.Left;
            if (vec.x > -0.1f && vec.x < 0.1f && vec.z > -0.1f && vec.z < 0.1f) return InputCommand.Neutral;
            if (vec.x >= 0.1f && vec.z > -0.1f && vec.z < 0.1f) return InputCommand.Right;
            
            if (vec.x <= -0.1f && vec.z >= 0.1f) return InputCommand.UpLeft;
            if (vec.x > -0.1f && vec.x < 0.1f && vec.z >= 0.1f) return InputCommand.Up;
            if (vec.x >= 0.1f && vec.z >= 0.1f) return InputCommand.UpRight;

            return InputCommand.Neutral;
        }
    }
}
