using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MessagePipe;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace UI
{
    public class GameOverViewController : MonoBehaviour
    {
        [SerializeField] private Button retryButton;
        [SerializeField] private Button titleButton;
        
        private CancellationTokenSource _cts;
        private IPublisher<GlobalEventData> _globalEventDataPublisher;

        private bool alreadyClicked = false;
        void Start()
        {
            _globalEventDataPublisher = GlobalMessagePipe.GetPublisher<GlobalEventData>();
            GlobalMessagePipe
                .GetSubscriber<ResultData>()
                .Subscribe(_ =>
                {
                    Debug.LogWarning("get result data");
                    _cts = InitializeCancellationTokenSource(_cts);
                    ShowGameOverView(_cts.Token).Forget();
                }).AddTo(this);

            retryButton.OnClickAsObservable()
                .Where(_ => !alreadyClicked)
                .ThrottleFirst(TimeSpan.FromSeconds(1))
                .Subscribe(_ =>
                {
                    alreadyClicked = true;
                    _globalEventDataPublisher.Publish(GlobalEventData.RestartGame);
                }).AddTo(this);
            titleButton.OnClickAsObservable()
                .Where(_ => !alreadyClicked)
                .ThrottleFirst(TimeSpan.FromSeconds(1))
                .Subscribe(_ =>
                {
                    alreadyClicked = true;
                    _globalEventDataPublisher.Publish(GlobalEventData.ToTitle);
                }).AddTo(this);
        }

        private async UniTaskVoid ShowGameOverView(CancellationToken token)
        {
            await GetComponent<RectTransform>().DOAnchorPosY(0, 1f, true).SetEase(Ease.OutBounce);
            
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.firstSelectedGameObject = retryButton.gameObject;
            EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
            retryButton.OnSelect(null);
        }
        
        private CancellationTokenSource InitializeCancellationTokenSource(CancellationTokenSource cts)
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = new CancellationTokenSource();

            return cts;
        }

        private void OnDisable()
        {
            /*
            _cts?.Cancel();
            _cts?.Dispose();
            */
        }
    }
}