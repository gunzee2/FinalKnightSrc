using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Characters.Actions.Behaviour.Conditionals
{
	[TaskCategory("EnemyConditonals")]
    public class IsCharacterDown : Conditional
    {
        public override TaskStatus OnUpdate()
        {
            return gameObject.GetComponent<ICharacterStateProvider>().CurrentActionState.Value == ActionState.Down ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}