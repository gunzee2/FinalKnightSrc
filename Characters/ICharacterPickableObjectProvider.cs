using UnityEngine;

namespace Characters
{
    public interface ICharacterPickableObjectProvider 
    {
        public GameObject PickingObject { get; }
        public GameObject UnderFootObject { get; }
    }
}
