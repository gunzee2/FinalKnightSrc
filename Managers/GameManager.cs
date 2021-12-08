using System;
using System.Threading;
using Characters;
using Characters.Damages;
using Chronos;
using Cysharp.Threading.Tasks;
using DI;
using Levels;
using MessagePipe;
using Rewired;
using UI;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GlobalClock _globalClock;

        private ISubscriber<GlobalEventData> _globalEventDataSubscriber;

        
        public BoolReactiveProperty isLogging = new BoolReactiveProperty(false);

        private IPublisher<ResultData> _resultDataPublisher;
        void Awake()
        {
            Application.targetFrameRate = 60;
        }


        private void Start()
        {
            isLogging.Subscribe(x =>
            {
                Debug.unityLogger.logEnabled = x;
            }).AddTo(this);
            
            _resultDataPublisher = GlobalMessagePipe.GetPublisher<ResultData>();
            _globalEventDataSubscriber = GlobalMessagePipe.GetSubscriber<GlobalEventData>();
            
            // もし死亡したキャラクターがプレーヤーだったら、ゲームオーバーイベントを発行する.
            GlobalMessagePipe.GetSubscriber<DeadData>()
                .AsObservable()
                .Where(x => x.characterType == CharacterType.Player)
                .Subscribe(x =>
                {
                    GameOverSequence(this.GetCancellationTokenOnDestroy(), x).Forget();
                }).AddTo(this);
            
            _globalEventDataSubscriber
                .AsObservable()
                .Where(x => x == GlobalEventData.StageClear)
                .Subscribe(_ =>
                {
                    StageClearSequence(this.GetCancellationTokenOnDestroy()).Forget();
                }).AddTo(this);
            
            // RestartGameイベントを受け取ったらリスタート
            _globalEventDataSubscriber
                .AsObservable()
                .Where(x => x == GlobalEventData.RestartGame)
                .Subscribe(_ =>
                {
                    Debug.LogWarning("restart");
                    RetryGameSequence(this.GetCancellationTokenOnDestroy()).Forget();
                }).AddTo(this);
            // ToTitleイベントを受け取ったらタイトル画面に戻る
            _globalEventDataSubscriber
                .AsObservable()
                .Where(x => x == GlobalEventData.ToTitle)
                .Subscribe(_ =>
                {
                    Debug.LogWarning("to title");
                    ToTitleSequence(this.GetCancellationTokenOnDestroy()).Forget();
                }).AddTo(this);
        }
        /// <summary>
        /// ステージクリア処理
        /// </summary>
        /// <param name="token"></param>
        private async UniTaskVoid StageClearSequence(CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token); 
            Debug.LogWarning("stage clear");
            _globalClock.localTimeScale = 0f;
            
            // コントローラマップをUI用に切り替え
            var player = ReInput.players.GetPlayer(0);
            player.controllers.maps.SetMapsEnabled(false, "InGame");
            player.controllers.maps.SetMapsEnabled(true, "UI");
        }

        /// <summary>
        /// ゲームオーバー画面表示用のメッセージを送信する処理
        /// </summary>
        /// <param name="token"></param>
        /// <param name="deadData"></param>
        private async UniTaskVoid GameOverSequence(CancellationToken token, DeadData deadData)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.75f), cancellationToken: token); 
            Debug.LogWarning("player dead");
            _globalClock.localTimeScale = 0f;
            
            // コントローラマップをUI用に切り替え
            var player = ReInput.players.GetPlayer(0);
            player.controllers.maps.SetMapsEnabled(false, "InGame");
            player.controllers.maps.SetMapsEnabled(true, "UI");
            
            // ゲームオーバー画面表示メッセージを送信
            var data = new ResultData{ AttackerCore = deadData.attackerCore };
            _resultDataPublisher.Publish(data);
        }
        
        #region UI Event Message
        
        /// <summary>
        /// UI画面からリトライイベントメッセージを受け取ったときの処理
        /// </summary>
        /// <param name="token"></param>
        private async UniTaskVoid RetryGameSequence(CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token); 
            
            var player = ReInput.players.GetPlayer(0);
            player.controllers.maps.SetMapsEnabled(true, "InGame");
            player.controllers.maps.SetMapsEnabled(false, "UI");
            
            SceneManager.LoadScene (SceneManager.GetActiveScene().name);
        }
        
        private async UniTaskVoid ToTitleSequence(CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token); 
            
            SceneManager.LoadScene (0);
        }
        #endregion

    }
}