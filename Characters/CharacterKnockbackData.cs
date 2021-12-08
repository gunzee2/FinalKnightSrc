using Characters.Damages;
using UnityEngine;

namespace Characters
{
    public struct CharacterKnockbackData
    {
        public KnockBack KnockBackDirection;
        public Vector3 AttackerPosition;
        public Vector3 AttackerFacingDirection;
        public float SlidePower;
        public float UpPower;
        public bool IsBlowAway;
    }
}
