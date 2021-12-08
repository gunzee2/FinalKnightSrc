using Characters.Actions;
using Chronos;
using DG.Tweening;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
namespace Characters.UI
{
    public class ModelSuperArmorColorChanger : SerializedMonoBehaviour
    {
        [SerializeField] private ICharacterStateProvider characterStateProvider;
        [SerializeField] private Renderer renderer;
    
        [SerializeField] private string propertyName;
        [SerializeField][ColorUsage(false, true)] private Color color = Color.red;
        [SerializeField] private Timeline timeLine;
    
        private Tween _tween;
        private Color _initialColor;
    
        // Start is called before the first frame update
        void Start()
        {
        
        
            _initialColor = renderer.material.GetColor(propertyName);
        
            Debug.Log(characterStateProvider);
            characterStateProvider.HaveSuperArmor.Where(x => x).Where(_ => characterStateProvider.CurrentActionState.Value != ActionState.Dead).Subscribe(_ =>
            {
                _tween?.Kill();
                _tween = renderer.material.DOColor(color, propertyName, 0.5f).OnUpdate(() => _tween.timeScale = timeLine.timeScale);
            }).AddTo(this);
            characterStateProvider.HaveSuperArmor.Where(x => !x).Subscribe(_ =>
            {
                renderer.material.SetColor(propertyName, _initialColor);
                _tween.Kill();
            }).AddTo(this);
        }
    }
}
