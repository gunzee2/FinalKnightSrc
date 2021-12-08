using UniRx;
using UnityEngine;

namespace Characters
{
    /// <summary>
    /// キャラクターの武器の表示制御
    /// </summary>
    public class CharacterWeaponController : MonoBehaviour
    {
        [SerializeField] private GameObject defaultWeaponModel;

        [SerializeField] private Transform weaponAttachmentTransform;

        private ICharacterPickableObjectProvider _pickableObjectProvider;
        private ICharacterEventProvider _characterEventProvider;

        void Start()
        {
            _pickableObjectProvider = GetComponent<ICharacterPickableObjectProvider>();
            _characterEventProvider = GetComponent<ICharacterEventProvider>();

            // アイテムを拾ったら、拾ったアイテムを手に持つ
            _characterEventProvider.OnGetPickupItem.Where(x => x.Type == ItemType.Thrower).Subscribe(x =>
                {
                    if (defaultWeaponModel)
                        defaultWeaponModel.SetActive(false);
                    x.EquipItem(weaponAttachmentTransform, Quaternion.identity, Vector3.zero);
                }
            ).AddTo(this);
            // アイテムを失ったら、元々持っていた武器を表示し直す
            _characterEventProvider.OnLostPickupItem.Subscribe(x =>
                {
                    if (defaultWeaponModel)
                        defaultWeaponModel.SetActive(true);
                }
            ).AddTo(this);

            // アイテムを投げる
            _characterEventProvider.OnThrowPickupItem.Subscribe(x =>
                {
                    // プレーヤーの向きに合わせて投げ物の向きを変える
                    var angles = transform.forward == Vector3.right ? new Vector3(90, 90, 0) : new Vector3(-90, 90, 0);
                    x.ThrowItem(angles, transform.forward, 10f);
                }
            ).AddTo(this);
        }
    }
}