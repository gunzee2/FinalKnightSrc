using Characters;
using Characters.Damages;
using MessagePipe;
using UI;
using UnityEngine;

namespace DI
{
    public class BCBMediaPipeInstaller : MonoBehaviour
    {
        void Awake()
        {
            var builder = new BuiltinContainerBuilder();
            builder.AddMessagePipe();
            builder.AddMessageBroker<CharacterCore>();
            builder.AddMessageBroker<DeadData>();
            builder.AddMessageBroker<ResultData>();
            builder.AddMessageBroker<GlobalEventData>();
            builder.AddMessageBroker<SoundManager.SoundEvent>();
            builder.AddMessageBroker<CameraShakeEventData>();

            var provider = builder.BuildServiceProvider();
            GlobalMessagePipe.SetProvider(provider);
        

        }

    }
}
