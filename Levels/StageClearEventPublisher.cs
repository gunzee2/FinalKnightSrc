using MessagePipe;
using Sirenix.OdinInspector;
using UI;
using UniRx;
using UnityEngine;

namespace Levels
{
    public class StageClearEventPublisher : SerializedMonoBehaviour
    {
        private IPublisher<GlobalEventData> _globalEventPublisher;

        [SerializeField] private IBattleEventProvider stageClearBattleEventProvider;

        
        
        // Start is called before the first frame update
        void Start()
        {
            _globalEventPublisher = GlobalMessagePipe.GetPublisher<GlobalEventData>();

            stageClearBattleEventProvider.OnAllEnemyDead.Subscribe(_ =>
            {
                _globalEventPublisher.Publish(GlobalEventData.StageClear);
            }).AddTo(this);
        }

    }
}
