using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Characters.Actions.Behaviour.Composites
{
    [TaskDescription("重み付きRandom Selector, weightsに指定された個数分しか選ばれない(順番は左から)ため注意")]
    [TaskIcon("{SkinColor}RandomSelectorIcon.png")]
    public class WeightedRandomSelector : Composite
    {

        public List<float> weights;
        
        [BehaviorDesigner.Runtime.Tasks.Tooltip("Seed the random number generator to make things easier to debug")]
        public int seed = 0;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("Do we want to use the seed?")]
        public bool useSeed = false;

        // A list of indexes of every child task. This list is used by the Fischer-Yates shuffle algorithm.
        private List<int> childIndexList = new List<int>();
        // The random child index execution order.
        private Stack<int> childrenExecutionOrder = new Stack<int>();
        // The task status of the last child ran.
        private TaskStatus executionStatus = TaskStatus.Inactive;

        public override void OnAwake()
        {
            // If specified, use the seed provided.
            if (useSeed) {
                Random.InitState(seed);
            }

            // Add the index of each child to a list to make the Fischer-Yates shuffle possible.
            childIndexList.Clear();
            for (int i = 0; i < children.Count; ++i) {
                childIndexList.Add(i);
            }
        }

        public override void OnStart()
        {
            // Randomize the indecies
            ShuffleChilden();
        }

        public override int CurrentChildIndex()
        {
            //Debug.Log($"weighted random :{childrenExecutionOrder.Peek()}");
            // Peek will return the index at the top of the stack.
            return childrenExecutionOrder.Peek();
        }

        public override bool CanExecute()
        {
            // Continue exectuion if no task has return success and indexes still exist on the stack.
            return childrenExecutionOrder.Count > 0 && executionStatus != TaskStatus.Success;
        }

        public override void OnChildExecuted(TaskStatus childStatus)
        {
            // Pop the top index from the stack and set the execution status.
            if (childrenExecutionOrder.Count > 0) {
                childrenExecutionOrder.Pop();
            }
            executionStatus = childStatus;
        }

        public override void OnConditionalAbort(int childIndex)
        {
            // Start from the beginning on an abort
            childrenExecutionOrder.Clear();
            executionStatus = TaskStatus.Inactive;
            ShuffleChilden();
        }

        public override void OnEnd()
        {
            // All of the children have run. Reset the variables back to their starting values.
            executionStatus = TaskStatus.Inactive;
            childrenExecutionOrder.Clear();
        }

        public override void OnReset()
        {
            // Reset the public properties back to their original values
            seed = 0;
            useSeed = false;
        }

        private void ShuffleChilden()
        {

            var list = childIndexList.Zip(weights, (x, w) => new {Value = x, Weight = w})
                .OrderBy(x =>
                {
                    var u = Random.value;
                    return Mathf.Pow(u, 1.0f / x.Weight);
                })
                .Select(x => x.Value);
            foreach (var x in list)
            {
                childrenExecutionOrder.Push(x);
            }
        }
    }
}