using System;
using UniRx;
using UnityEngine;

namespace Characters.Inputs
{
    public enum InputCommand
    {
        NONE,
        DownLeft,
        Down,
        DownRight,
        Left,
        Neutral,
        Right,
        UpLeft,
        Up,
        UpRight,
        Attack,
        AttackRelease,
        DownAttack,
        Jump,
        MegaCrash,
        Block,
        EnemyRotate,
        MoveAttack,
        SlashAttack
    }

    public interface ICommandEventProvider
    {
        IObservable<InputCommand> OnCommandInput { get; }
        
        IReadOnlyReactiveProperty<Vector3> AnalogueMovementInput { get; }
    }
}