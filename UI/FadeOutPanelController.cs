using DG.Tweening;
using MessagePipe;
using UniRx;
using UnityEngine;
namespace UI
{
    public class FadeOutPanelController : MonoBehaviour
    {

        [SerializeField] private float duration = 0.5f;
    
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        // Start is called before the first frame update
        void Start()
        {
            GlobalMessagePipe.GetSubscriber<GlobalEventData>().AsObservable()
                .Where(x => x == GlobalEventData.ToTitle || x == GlobalEventData.RestartGame)
                .Subscribe(
                    _ =>
                    {
                        _canvasGroup.DOFade(1, duration);
                    }).AddTo(this);
        }

    }
}