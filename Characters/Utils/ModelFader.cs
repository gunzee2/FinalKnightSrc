using System;
using System.Threading;
using Characters.Actions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Characters.Utils
{
    public class ModelFader : SerializedMonoBehaviour
    {
        [SerializeField] private GameObject modelGO;
        [SerializeField] private GameObject rootGO; // フェードアウトした後に削除するGameObject
        [SerializeField] private Material fadeMaterial;

        [SerializeField] private string propertyName;
        [SerializeField] private float fadeDelayDuration;
        [SerializeField] private float fadeDuration;

        [SerializeField] private ICharacterStateProvider _characterStateProvider;

        private void Start()
        {
            if (_characterStateProvider == null) return;

            _characterStateProvider.CurrentActionState.Where(x => x == ActionState.Dead).Subscribe(_ => FadeOut())
                .AddTo(this);
        }

        public void FadeOut()
        {
            // Destroy時にキャンセルされるCancellationTokenを取得
            var token = this.GetCancellationTokenOnDestroy();

            FadeSequence(token).Forget();
        }

        async UniTaskVoid FadeSequence(CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(fadeDelayDuration), cancellationToken: token);
                modelGO.layer = 11; //ZTestTransparentレイヤーを指定
                var renderer = modelGO.GetComponent<Renderer>();
                renderer.material = fadeMaterial;
                await renderer.material.DOColor(new Color(0, 0, 0, 0), propertyName, fadeDuration)
                    .WithCancellation(token);

                Destroy(rootGO);
            }
            catch (OperationCanceledException ex)
            {
                Debug.Log("task canceled.");
            }
        }
    }
}