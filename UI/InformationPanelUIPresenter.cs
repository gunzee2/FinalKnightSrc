using System;
using System.Threading;
using Characters;
using Characters.Actions;
using Cysharp.Threading.Tasks;
using MessagePipe;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InformationPanelUIPresenter : SerializedMonoBehaviour 
    {
        [SerializeField] private bool isEnemyInformationPanel;
        [SerializeField] private float displayDuration = 3f;
        #region View

        [Header("View")] [SerializeField] private TMP_Text nameText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private Image healthBarImage;
        [SerializeField] private Image chargeBarImage;
        [SerializeField] private MMProgressBar _mmProgressBar;
        

        #endregion

        #region Model

        [Header("Model")] 
        
        [SerializeField] private CharacterCore characterCore;

        #endregion

        private CanvasGroup _canvasGroup;

        private IDisposable _healthBarDisposable;
        private IDisposable _chargeBarDisposable;
        private IDisposable _actionStateDisposable;

        private IDisposable _canvasGroupDisposable;

        private CancellationTokenSource _cts;



        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _mmProgressBar.Initialization();
        }


        private void Start()
        {
            
            if (isEnemyInformationPanel)
            {
                //MessageBroker.Default.Receive<CharacterCore>()
                GlobalMessagePipe.GetSubscriber<CharacterCore>().Subscribe(x =>
                    {
                        SetCharacterCore(x);
                        
                        // 表示後数秒で消える.
                        _canvasGroup.alpha = 1f;
                        _canvasGroupDisposable?.Dispose();
                        _canvasGroupDisposable = Observable.Timer(TimeSpan.FromSeconds(displayDuration)).Subscribe(_ =>
                        {
                            _canvasGroup.alpha = 0;
                        }).AddTo(this);
                    }).AddTo(this);
            }
            
            if (characterCore == null) return;

            SetInformationData(characterCore.GetComponent<ICharacterInformationProvider>(), characterCore.GetComponent<ICharacterStateProvider>());
            
        }

        private void SetInformationData(ICharacterInformationProvider characterInformation, ICharacterStateProvider characterState)
        {
            nameText.text = characterInformation.Name;
            portraitImage.sprite = characterInformation.PortraitImage;

            /*
            _healthBarDisposable = characterInformation
                .HealthRatio
                .Subscribe(x => { healthBarImage.fillAmount = x; })
                .AddTo(characterCore);
            */
            
            _mmProgressBar.SetBar01(characterInformation.HealthRatio.Value);
            _healthBarDisposable = characterInformation
                .HealthRatio
                .SkipLatestValueOnSubscribe()
                .Subscribe(x =>
                {
                    Debug.Log($" update health ratio {_mmProgressBar.BarProgress} => {x}", gameObject);
                    _mmProgressBar.UpdateBar01(x);
                })
                .AddTo(characterCore);
            
            _chargeBarDisposable = characterInformation
                .ChargeRatio
                .Subscribe(x => { chargeBarImage.fillAmount = x; })
                .AddTo(characterCore);

            _actionStateDisposable = characterState
                .CurrentActionState
                .Where(x => x == ActionState.Damage || x == ActionState.Down)
                .Subscribe(_ =>
                {
                    _cts = InitializeCancellationTokenSource(_cts);

                    AnimatePortraitOnDamaged(characterInformation, _cts.Token).Forget();
                }).AddTo(this);
        }

        private void SetCharacterCore(CharacterCore core)
        {
            characterCore = core;

            _healthBarDisposable?.Dispose();
            _chargeBarDisposable?.Dispose();
            _actionStateDisposable?.Dispose();

            SetInformationData(characterCore.GetComponent<ICharacterInformationProvider>(), characterCore.GetComponent<ICharacterStateProvider>());
        }

        private async UniTaskVoid AnimatePortraitOnDamaged(ICharacterInformationProvider characterInformation,CancellationToken cancellationToken)
        {
            try
            {
                //Debug.Log("animation Start");
                portraitImage.sprite = characterInformation.PortraitDamagedImage;
                await UniTask.Delay(TimeSpan.FromSeconds(0.05f), cancellationToken: cancellationToken);
                portraitImage.sprite = characterInformation.PortraitImage;
                await UniTask.Delay(TimeSpan.FromSeconds(0.05f), cancellationToken: cancellationToken);
                portraitImage.sprite = characterInformation.PortraitDamagedImage;
                await UniTask.Delay(TimeSpan.FromSeconds(0.05f), cancellationToken: cancellationToken);
                portraitImage.sprite = characterInformation.PortraitImage;
                await UniTask.Delay(TimeSpan.FromSeconds(0.05f), cancellationToken: cancellationToken);
                portraitImage.sprite = characterInformation.PortraitDamagedImage;
                await UniTask.Delay(TimeSpan.FromSeconds(0.05f), cancellationToken: cancellationToken);
                portraitImage.sprite = characterInformation.PortraitImage;
                await UniTask.Delay(TimeSpan.FromSeconds(0.05f), cancellationToken: cancellationToken);
                portraitImage.sprite = characterInformation.PortraitDamagedImage;
                await UniTask.Delay(TimeSpan.FromSeconds(0.05f), cancellationToken: cancellationToken);
                portraitImage.sprite = characterInformation.PortraitImage;
                await UniTask.Delay(TimeSpan.FromSeconds(0.05f), cancellationToken: cancellationToken);
                portraitImage.sprite = characterInformation.PortraitDamagedImage;
                await UniTask.Delay(TimeSpan.FromSeconds(0.05f), cancellationToken: cancellationToken);
                
                portraitImage.sprite = characterInformation.PortraitImage;
                //Debug.Log("animation End");
                
            }
            catch (OperationCanceledException)
            {
                //Debug.Log("animation Canceled.");
            }
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private CancellationTokenSource InitializeCancellationTokenSource(CancellationTokenSource cts)
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = new CancellationTokenSource();

            return cts;
        }
    }
}