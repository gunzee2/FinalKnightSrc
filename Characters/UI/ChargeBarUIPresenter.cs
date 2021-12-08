using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Characters.UI
{
    public class ChargeBarUIPresenter : MonoBehaviour
    {
        [SerializeField] private Image chargeBarView;
        [SerializeField] private string characterTag;

        private void Start()
        {
            IChargeEnergyReadable chargeEnergyReadable = null;
        
            var characterGO = GameObject.FindWithTag(characterTag);
            if (characterGO != null)
            {
                chargeEnergyReadable = characterGO.GetComponent<IChargeEnergyReadable>();
            }

            UpdateChargeBar(chargeEnergyReadable, characterGO);

            // もしプレーヤーが見つからなくなった(削除された)時は毎フレーム探して見つけたら再度アタッチする.
            this.UpdateAsObservable().Where(_ => chargeEnergyReadable == null).Subscribe(_ =>
            {
                chargeEnergyReadable = GameObject.FindWithTag(characterTag).GetComponent<IChargeEnergyReadable>();
            
                UpdateChargeBar(chargeEnergyReadable, characterGO);
            }).AddTo(this);
        }

        private void UpdateChargeBar(IChargeEnergyReadable chargeEnergyReadable, GameObject playerGO)
        {
            chargeEnergyReadable?.ChargeRatio.Subscribe(x =>
            {
                chargeBarView.fillAmount = x;
            }).AddTo(playerGO);
        }
    }
}
