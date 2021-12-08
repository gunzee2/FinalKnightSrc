using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DarkTonic.MasterAudio;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class TitleSceneController : MonoBehaviour
{
    [SerializeField] private PlayableDirector _playableDirector;
    
    
    private bool alreadySubmitButtonOn = false;

    private AsyncOperation loadSceneAsyncOperation;
    
    // Start is called before the first frame update
    void Start()
    {
        var player = ReInput.players.GetPlayer(0);
        player.controllers.maps.SetMapsEnabled(false, "InGame");
        player.controllers.maps.SetMapsEnabled(true, "UI");


        var mouseDownObservable = this.ObserveEveryValueChanged(_ => Input.GetMouseButtonDown(0))
            .Where(x => x)
            .Where(_ => !alreadySubmitButtonOn)
            .Subscribe(_ => { StartSceneTransitionSequence(); }).AddTo(this);
        
        this.ObserveEveryValueChanged(_ => player.GetButtonDown("UISubmit"))
            //.Amb(mouseDownObservable)
            .Where(x => x)
            .Where(_ => !alreadySubmitButtonOn)
            .Subscribe(_ =>
            {
                StartSceneTransitionSequence();
            }).AddTo(this);

    }

    private void StartSceneTransitionSequence()
    {
        alreadySubmitButtonOn = true;
        MasterAudio.PlaySound("door_open2");
        
        loadSceneAsyncOperation = SceneManager.LoadSceneAsync(sceneBuildIndex: 1);
        loadSceneAsyncOperation.allowSceneActivation = false;
        
        _playableDirector.Play();
    }

    public void SceneTransition()
    {
        WaitLoadScene(this.GetCancellationTokenOnDestroy()).Forget();
    }
    
    private async UniTaskVoid WaitLoadScene(CancellationToken token)
    {
        loadSceneAsyncOperation.allowSceneActivation = true;
        await loadSceneAsyncOperation.WithCancellation(token);
    }
}