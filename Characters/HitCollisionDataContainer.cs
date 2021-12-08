using System;
using Characters.Actions;
using Characters.Damages;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Characters
{
    public class HitCollisionDataContainer : SerializedMonoBehaviour, IHitCollisionDataProvider
    {
        [SerializeField] private GameObject rootGO;

        [SerializeField] private IDamageApplicable _applicable;

        public Guid PreviousDamageId => _applicable.PreviousDamageId;
        public IReadOnlyReactiveProperty<bool> IsInvincible => _applicable.IsInvincible;
        public string CharacterTag => rootGO.tag;

        public IDamageApplicable DamageApplicable => _applicable;
        public AttackType AttackType { get; set; }

        public Vector3 HitPosition { get; set; }
    }
}