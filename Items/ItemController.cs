using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using TMPro;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

public enum ItemType
{
    None,
    Food,
    Thrower
}
public class ItemController : MonoBehaviour
{
    public string ItemName => _itemName;
    [SerializeField] private string _itemName;
    
    public ItemType Type => _type;
    [SerializeField]private ItemType _type;

    public int Value => _value;
    [SerializeField] private int _value;

    public Light Light => _light;
    [SerializeField] private Light _light;

    public Collider ObjCollider => _objCollider;
    [SerializeField] private Collider _objCollider;
    
    public Collider AttackCollider => _attackCollider;
    [SerializeField] private Collider _attackCollider;

    private Rigidbody _rigidbody;

    private IDisposable throwTimerDisposable;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void EquipItem(Transform parentTransform, Quaternion rotation, Vector3 position)
    {
        transform.SetParent(parentTransform);
        transform.localRotation = rotation;
        transform.localPosition = position;
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
        Light.enabled = false;
        ObjCollider.enabled = false;
        tag = "Untagged";
    }

    public void DropItem()
    {
        var dropRotation = 0f;
        if (_rigidbody.velocity.x > 0)
            dropRotation = 360f;
        else
            dropRotation = -360f;
        _rigidbody.isKinematic = false;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.useGravity = true;
        _rigidbody.AddForce((Vector3.up * 3f + new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f))), ForceMode.Impulse);
        _rigidbody.AddTorque(new Vector3(0, 0, dropRotation), ForceMode.Impulse);
        Light.enabled = true;
        AttackCollider.enabled = false;
        ObjCollider.enabled = true;
        tag = "Item";
        
        throwTimerDisposable?.Dispose();
    }

    public void ThrowItem(Vector3 rotation, Vector3 direction, float speed)
    {
        transform.SetParent(null);
        _rigidbody.isKinematic = false;
        transform.eulerAngles = rotation;
        _rigidbody.velocity = direction * speed;
        Light.enabled = true;
        AttackCollider.enabled = true;

        // 一定時間何にも当たらず飛んでいたら削除する
        throwTimerDisposable = Observable.Timer(TimeSpan.FromSeconds(4f)).Subscribe(_ =>
        {
            Destroy(gameObject);
        }).AddTo(this);
    }
    
}