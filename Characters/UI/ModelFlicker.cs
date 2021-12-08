using Characters.Actions;
using Chronos;
using DG.Tweening;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Characters.UI
{
    /// <summary>
    /// モデルを点滅させる
    /// </summary>
    public class ModelFlicker : SerializedMonoBehaviour
    {
        [SerializeField] private ICharacterStateProvider characterStateProvider;
        [SerializeField] private string propertyName;

        [SerializeField] [ColorUsage(false, true)]
        private Color flickerColor = Color.white;

        [SerializeField] private int overShootCount = 20;
        [SerializeField] private Timeline timeLine;

        private Renderer _renderer;

        private Tween _tween;
        private Color _initialColor;

        [SerializeField]
        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        private void Start()
        {
            _initialColor = _renderer.material.GetColor(propertyName);

            Debug.Log(characterStateProvider);
            characterStateProvider.IsInvincible.Where(x => x)
                .Where(_ => characterStateProvider.CurrentActionState.Value != ActionState.Dead).Subscribe(_ =>
                    {
                        _tween?.Kill();
                        _tween = _renderer.material.DOColor(flickerColor, propertyName, 1f)
                            .SetEase(Ease.Flash, overShootCount)
                            .SetLoops(-1, LoopType.Yoyo).OnUpdate(() => _tween.timeScale = timeLine.timeScale);
                    }
                ).AddTo(this);
            characterStateProvider.IsInvincible.Where(x => !x).Subscribe(_ =>
                {
                    _renderer.material.SetColor(propertyName, _initialColor);
                    _tween.Kill();
                }
            ).AddTo(this);
        }
    }
}