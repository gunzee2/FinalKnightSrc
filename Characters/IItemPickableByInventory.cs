using UnityEngine;

namespace Characters
{
        public interface IItemPickableByInventory
        {
        
                GameObject PickingObject { get; }
                void PickupItemByInventory(GameObject go);
        }
}
