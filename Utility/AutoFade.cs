using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class AutoFade : MonoBehaviour
{
    [FormerlySerializedAs("renderer")] [SerializeField] private Renderer _renderer;
    [SerializeField] private float duration;
    

    
    // Start is called before the first frame update
    void Start()
    {
        var color = _renderer.material.color;
        _renderer.material.DOColor(new Color(color.r, color.g, color.b, 0), duration).SetEase(Ease.InQuint);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
