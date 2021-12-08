using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Characters.UI
{
    public class SwordFlasher : SerializedMonoBehaviour
    {
        [SerializeField] private string propertyName;
        [SerializeField][ColorUsage(false, true)] private Color flashColor = Color.white;
        [SerializeField] private ICharacterInformationProvider _characterInformationProvider;
    
        private Renderer _renderer;
        private Color _initialColor;

    
        [SerializeField]

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        private void Start()
        {
            _initialColor = _renderer.material.GetColor(propertyName);

            _characterInformationProvider.ChargeRatio.Subscribe(x =>
            {
                _renderer.material.SetColor(propertyName, Color.Lerp(_initialColor, flashColor, x));
            }).AddTo(this);
    }
    }
}
