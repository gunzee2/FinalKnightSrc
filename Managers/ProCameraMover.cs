using Characters;
using Com.LuisPedroFonseca.ProCamera2D;
using UniRx;
using UnityEngine;

namespace Managers
{
    public class ProCameraMover : MonoBehaviour
    {
        [SerializeField] ProCamera2DShake _cameraShaker;
        // Start is called before the first frame update
        void Start()
        {
            MessageBroker.Default.Receive<CharacterCore>().Subscribe(x =>
            {
                _cameraShaker.Shake(0);
            }).AddTo(this);

        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
