using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DarkTonic.MasterAudio;
using DG.Tweening;
using MessagePipe;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class StageClearViewController : MonoBehaviour
    {
        
        private CancellationTokenSource _cts;
        private IPublisher<GlobalEventData> _globalEventDataPublisher;

        [SerializeField] private TMP_Text _stageClearText;
        [SerializeField] private TMP_Text _thanksText;
        [SerializeField] private TMP_Text _presentedText;

        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image logoImage;
        [SerializeField] private Button titleButton;
        

        private bool alreadyClicked = false;
        void Start()
        {
            _globalEventDataPublisher = GlobalMessagePipe.GetPublisher<GlobalEventData>();
            
            GlobalMessagePipe
                .GetSubscriber<GlobalEventData>()
                .AsObservable()
                .Where(x => x == GlobalEventData.StageClear)
                .Subscribe(_ =>
                {
                    _cts = InitializeCancellationTokenSource(_cts);
                    ShowStageClearView(_cts.Token).Forget();
                }).AddTo(this);
            titleButton.OnClickAsObservable()
                .Where(_ => !alreadyClicked)
                .ThrottleFirst(TimeSpan.FromSeconds(1))
                .Subscribe(_ =>
                {
                    alreadyClicked = true;
                    _globalEventDataPublisher.Publish(GlobalEventData.ToTitle);
                }).AddTo(this);

            _stageClearText.maxVisibleCharacters = 0;
            _thanksText.maxVisibleCharacters = 0;
            _presentedText.maxVisibleCharacters = 0;
            logoImage.transform.localScale = Vector3.zero;
            titleButton.transform.localScale = Vector3.zero;
        }

        private async UniTaskVoid ShowStageClearView(CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token);
            GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

            MasterAudio.StopPlaylist("PlaylistController");
            MasterAudio.PlaySound("clear_jingle");
            await backgroundImage.DOFade(0.75f, 0.5f).WithCancellation(token);
            await _stageClearText.DOMaxVisibleCharacters(_stageClearText.text.Length, 1f).WithCancellation(token);
            
            await _thanksText.DOMaxVisibleCharacters(_thanksText.text.Length, 1f).WithCancellation(token);
            await _presentedText.DOMaxVisibleCharacters(_presentedText.text.Length, 0.5f).WithCancellation(token);
            logoImage.enabled = true;
            await logoImage.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce).WithCancellation(token);
            titleButton.gameObject.SetActive(true);
            await titleButton.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce).WithCancellation(token);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.firstSelectedGameObject = titleButton.gameObject;
            EventSystem.current.SetSelectedGameObject(titleButton.gameObject);
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
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}