using BehaviorDesigner.Runtime.Tasks;

namespace Characters.Actions.Behaviour.Conditionals
{
    public class HavePickedObject : Conditional
    {
        private IItemPickableByInventory _itemPickableByInventory;
        public override TaskStatus OnUpdate()
        {
            var itemPickableByInventory = GetComponent<CharacterCore>();
            
            return itemPickableByInventory.PickingObject ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}