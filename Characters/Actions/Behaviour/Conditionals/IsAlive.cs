using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Characters.Actions.Behaviour.Conditionals
{
	[TaskCategory("EnemyConditonals")]
    public class IsAlive : Conditional
    {
        public override TaskStatus OnUpdate()
        {
            return GetComponent<CharacterCore>().CurrentHealth.Value > 0 ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}