using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Characters.UI
{
    public class HealthBarUIPresenter : MonoBehaviour
    {
        [SerializeField] private Image healthBarView;
        [SerializeField] private string characterTag;

        private void Start()
        {
            IHealthReadable healthReadable = null;
        
            var characterGO = GameObject.FindWithTag(characterTag);
            if (characterGO != null)
            {
                healthReadable = characterGO.GetComponent<IHealthReadable>();
            }

            UpdateHealthBar(healthReadable, characterGO);

            // もしプレーヤーが見つからなくなった(削除された)時は毎フレーム探して見つけたら再度アタッチする.
            this.UpdateAsObservable().Where(_ => healthReadable == null).Subscribe(_ =>
            {
                healthReadable = GameObject.FindWithTag(characterTag).GetComponent<IHealthReadable>();
            
                UpdateHealthBar(healthReadable, characterGO);
            }).AddTo(this);
        }

        private void UpdateHealthBar(IHealthReadable healthReadable, GameObject playerGO)
        {
            healthReadable?.HealthRatio.Subscribe(x =>
            {
                healthBarView.fillAmount = x;
            }).AddTo(playerGO);
        }
    }
}
