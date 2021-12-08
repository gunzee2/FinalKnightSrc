using System;
using System.Collections.Generic;
using Characters.Actions;
using Characters.Damages;
using Chronos;
using MessagePipe;
using Sirenix.OdinInspector;
using UniRx;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Characters.Obstacles
{
    public class ObstacleDestructor : SerializedMonoBehaviour
    {
        public enum ObstacleType
        {
            None,
            Wood
        }
        private ICharacterEventProvider _characterEventProvider;
        private const float ROTATION_Y = -90f;
        
        private IPublisher<SoundManager.SoundEvent> _publisher;
        
        public IObservable<Vector3> OnRotate => _onRotate;
        private readonly Subject<Vector3> _onRotate = new Subject<Vector3>();

        [SerializeField] private GameObject model;
        [SerializeField] private GameObject fractureObjPrefab;

        [SerializeField] private List<GameObject> dropItems;
        [SerializeField] private ObstacleType obstacleType;
        [SerializeField] private float power = 1f;

        [FormerlySerializedAs("useModelTransform")] [SerializeField] private bool useModelRotation = true;
        
        
        

        
        // Start is called before the first frame update
        void Start()
        {
            _publisher = GlobalMessagePipe.GetPublisher<SoundManager.SoundEvent>();
            _characterEventProvider = GetComponent<ICharacterEventProvider>();
        
            // ノックバック.
            _characterEventProvider.OnDamaged.Subscribe(x =>
            {
                if (x.attackType == AttackType.ThrowItem) return; // 投げ物だった場合は何もしない
                var vec = Vector3.zero;
                if (transform.position.x < x.attackerCore.transform.position.x)
                {
                    // 左に飛ぶ
                    vec = (Vector3.left * 0.75f * power) + Vector3.up;
                }
                else
                {
                    // 右に飛ぶ
                    vec = (Vector3.right * 0.75f * power) + Vector3.up;
                }
                DropItem();
                CreateFractureObject(vec);
                PlayBreakSound();

            }).AddTo(this);
        
        }

        private void PlayBreakSound()
        {
            switch (obstacleType)
            {
                case ObstacleType.Wood:
                    _publisher.Publish(new SoundManager.SoundEvent { Name = SoundManager.SoundEvent.SoundName.HitHeavy, Type = SoundManager.SoundEvent.SoundType.Play});
                    _publisher.Publish(new SoundManager.SoundEvent { Name = SoundManager.SoundEvent.SoundName.BarrelBreak, Type = SoundManager.SoundEvent.SoundType.Play});
                    break;
            }
        }

        private void DropItem()
        {
            if (dropItems.Count <= 0) return;
            
            var item = dropItems[Random.Range(0, dropItems.Count)];
            Instantiate(item, transform.position + Vector3.up, transform.rotation);
        }

        private void CreateFractureObject(Vector3 force)
        {
            GameObject obj;
            if(useModelRotation)
                obj = Instantiate(fractureObjPrefab, model.transform.position, model.transform.rotation);
            else 
                obj = Instantiate(fractureObjPrefab, model.transform.position, fractureObjPrefab.transform.rotation);
            
            foreach (Transform child in obj.transform)
            {
                var rb = child.GetComponent<Rigidbody>(); 
                rb.maxAngularVelocity = 30f;
                rb.AddForce(force * 5f, ForceMode.Impulse);
                rb.AddTorque(Vector3.forward * Random.Range(-3f, 3f) * -Mathf.Sign(force.x), ForceMode.Impulse);
            }
            Destroy(gameObject);
        }
        
        private bool IsFacingRight(Vector3 direction)
        {
            return direction.normalized == Vector3.right;
        }

    }
}