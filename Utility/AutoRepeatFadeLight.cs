using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class AutoRepeatFadeLight : MonoBehaviour
{
    [SerializeField] private float power;
    [SerializeField] private float duration = 0.5f;
    

    private Tween _tween;

    void Start()
    {
        var light = GetComponent<Light>();
        _tween = light.DOIntensity(power, duration).SetLoops(-1, LoopType.Yoyo);
        

    }

    private void OnDestroy()
    {
        _tween.Kill();
    }
}
