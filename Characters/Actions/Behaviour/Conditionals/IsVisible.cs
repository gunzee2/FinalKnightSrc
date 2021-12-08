using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Characters.Actions.Behaviour.Conditionals
{
	[TaskCategory("EnemyConditonals")]
    public class IsVisible : Conditional
    {
        public Renderer targetRenderer;
        public override TaskStatus OnUpdate()
        {
            return targetRenderer.isVisible ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}