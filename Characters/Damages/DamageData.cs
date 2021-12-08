using System;
using Characters.Actions;

namespace Characters.Damages
{
    public enum KnockBack
    {
        None,
        Forward,
        Reverse,
        DistanceAway
    }
    public struct DamageData
    {
        public AttackType attackType;
        public CharacterCore attackerCore;
        public int damageValue;
        public KnockBack knockBack;
        public bool isBlowAway;
        public Guid damageId;
    }
}
