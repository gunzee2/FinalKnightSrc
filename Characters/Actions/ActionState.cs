using UniRx;
using UnityEngine;

namespace Characters.Actions
{
    public enum ActionState 
    {
        Idle,
        Move,
        Pickup,
        GroundAttack,
        MegaCrash,
        Repelled,
        WaitNextAttack,
        Jump,
        JumpAttack,
        Landing,
        Block,
        BlockSuccess,
        BlockCancel,
        Damage,
        KnockBack,
        Down,
        Tired,
        Standing,
        MoveAttack,
        ThrowItem,
        Dead,
        Freeze
    }

    [System.Serializable]
    public class ActionStateReactiveProperty : ReactiveProperty<ActionState>
    {
        public ActionStateReactiveProperty(){}
        public ActionStateReactiveProperty(ActionState initialValue) : base (initialValue) {}

    }
}
