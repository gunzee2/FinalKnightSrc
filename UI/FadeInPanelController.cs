using DG.Tweening;
using UnityEngine;
namespace UI
{
    public class FadeInPanelController : MonoBehaviour
    {

        [SerializeField] private float duration = 2f;
    
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 1;
        }

        // Start is called before the first frame update
        void Start()
        {
            _canvasGroup.DOFade(0, duration);
        }

    }
}
