using System;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

namespace Characters
{
    [Serializable][InlineProperty]
    public struct CharacterStatus
    {
        public int MaxHealth;
        
        [Title("Damage Value")]
        [FormerlySerializedAs("NormalAttack1Damage")]
        [LabelText("Combo1")] 
        public int Combo1Damage;
        
        [FormerlySerializedAs("NormalAttack2Damage")]
        [LabelText("Combo2")] 
        public int Combo2Damage;
        
        [FormerlySerializedAs("NormalAttack3Damage")]
        [LabelText("Combo3")] 
        public int Combo3Damage;
        
        [LabelText("Jump1")]
        public int JumpAttack1Damage;
        
        [LabelText("Jump2")]
        public int JumpAttack2Damage;
        
        [LabelText("ChargeAttack")]
        public int ChargeAttackDamage;
        [LabelText("MegaCrash")]
        public int MegaCrashDamage;
        public int MegaCrashCost;
        
        [Title("Movement")]
        public float MoveSpeedX;
        public float MoveSpeedZ;
        public float JumpPower;
        public float DashSpeedMin;
        public float DashSpeedMax;
    }
}