using Characters.Actions;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Characters.FX
{
    public class ChargeFXPlayer : SerializedMonoBehaviour
    {
        [SerializeField] private ParticleSystem particleSystem;
        [SerializeField] private IAttackEventProvider attackEventProvider;
        [SerializeField] private ICharacterInformationProvider _characterInformationProvider;


    
        // Start is called before the first frame update
        void Start()
        {
            attackEventProvider.OnChargeStart.Subscribe(_ =>
            {
                particleSystem.Play();
            }).AddTo(this);
            attackEventProvider.OnChargeEnd.Subscribe(_ =>
            {
                particleSystem.Stop();
            }).AddTo(this);

            _characterInformationProvider.ChargeRatio.Subscribe(x =>
            {
                var particleSystemEmission = particleSystem.emission;
                particleSystemEmission.rateOverTime = Mathf.Lerp(0, 200, x);
            }).AddTo(this);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
