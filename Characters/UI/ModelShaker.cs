using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using Characters.Actions;
using DG.Tweening;
using UniRx;
using UnityEngine;
using Utility;

public class ModelShaker : MonoBehaviour
{
    [SerializeField] private Vector3 punch = Vector3.right;
    [SerializeField] private int vibrato = 5;

    [SerializeField] private Transform modelTransform;

    private ICharacterStateProvider _characterStateProvider;
    private ICharacterEventProvider _characterEventProvider;
    private Vector3 initialPosition;

    private Tweener _tweener;
    
    // Start is called before the first frame update
    void Awake()
    {
        _characterStateProvider = GetComponent<ICharacterStateProvider>();
        _characterEventProvider = GetComponent<ICharacterEventProvider>();
        initialPosition = modelTransform.localPosition;
    }

    private void Start()
    {
        _characterEventProvider.OnDamaged.Where(_ => _characterStateProvider.CurrentActionState.Value == ActionState.Damage).Subscribe(_ =>
        {
            Debug.Log("Damage Shake Start", gameObject);
            _tweener?.Pause();
            _tweener = modelTransform.DOPunchPosition(punch, 1f, vibrato, 0f, false).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
        }).AddTo(this);
        
        _characterStateProvider.CurrentActionState.Pairwise().Where(x => x.Previous == ActionState.Damage && x.Current != ActionState.Damage && x.Current != ActionState.KnockBack).Subscribe(_ =>
        {
            if (_tweener == null) return;

            Debug.Log("Damage Shake Stop", gameObject);
            _tweener.Pause();
            modelTransform.localPosition = initialPosition;
        }).AddTo(this);
        
        _characterStateProvider.CurrentActionState.Where(x => x == ActionState.KnockBack).Subscribe(_ =>
        {
            Debug.Log("Knockback Shake Start", gameObject);
            _tweener?.Pause();
            _tweener = modelTransform.DOPunchPosition(punch, 1f, vibrato, 0f, false).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
        }).AddTo(this);
        
        _characterStateProvider.CurrentActionState.Pairwise().Where(x => x.Previous == ActionState.KnockBack && x.Current != ActionState.KnockBack && x.Current != ActionState.Damage).Subscribe(_ =>
        {
            if (_tweener == null) return;
            
            Debug.Log("Knockback Shake Stop", gameObject);
            _tweener.Pause();
            modelTransform.localPosition = initialPosition;
        }).AddTo(this);
    }
}
