using System;
using Characters.Actions;
using Characters.Damages;
using UniRx;
using UnityEngine;

namespace Characters
{
    public interface IHitCollisionDataProvider
    {

        string CharacterTag { get; }
        
        IDamageApplicable DamageApplicable { get; }
        
        public AttackType AttackType { get; set; }
        
        public Vector3 HitPosition { get; set; }
        
    }
}