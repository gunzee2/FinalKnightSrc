using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [SerializeField] private float duration;
    
    // Start is called before the first frame update
    void Start()
    {
        Observable.Timer(TimeSpan.FromSeconds(duration)).Subscribe(_ =>
        {
            Destroy(gameObject);
        }).AddTo(this);
    }

}
