using Characters.Actions;
using Characters.Damages;
using Com.LuisPedroFonseca.ProCamera2D;
using MessagePipe;
using UniRx;
using UniRx.Diagnostics;
using UnityEngine;

namespace Cameras
{
    public class CameraShaker : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

            GlobalMessagePipe.GetSubscriber<CameraShakeEventData>()
                .AsObservable()
                .Debug()
                .Subscribe(x =>
                {
                    if(x.ShakeType == CameraShakeEventData.CameraShakeType.Heavy)
                        ProCamera2DShake.Instance.Shake("HeavyShakePreset");
                }).AddTo(this);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}