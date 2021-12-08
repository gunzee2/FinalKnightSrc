using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using UnityEngine;

namespace Characters.Actions.Behaviour.Actions
{
	[TaskCategory("EnemyActions")]
	public class PickupObjectByInventory : Action
	{
		public GameObject PickupObject;

		private IItemPickableByInventory _itemPickableByInventory;
		
		public override void OnStart()
		{
			_itemPickableByInventory = GetComponent<CharacterCore>();
		}

		public override TaskStatus OnUpdate()
		{
			var go = GameObject.Instantiate(PickupObject, transform.position, Quaternion.identity);
			_itemPickableByInventory.PickupItemByInventory(go);
			return TaskStatus.Success;
		}
	}
}