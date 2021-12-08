using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public interface IBattleAreaTriggerEventProvider 
{
        public IObservable<Unit> OnEnterTrigger { get; }
}
