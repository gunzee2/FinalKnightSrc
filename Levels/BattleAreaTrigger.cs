using System;
using Com.LuisPedroFonseca.ProCamera2D;
using Levels;
using UniRx;
using UnityEngine;

namespace Cameras
{
    public class BattleAreaTrigger : BaseTrigger, IBattleAreaTriggerEventProvider
    {
        public IObservable<Unit> OnEnterTrigger => _onEnterTrigger;
        private readonly Subject<Unit> _onEnterTrigger = new Subject<Unit>();

        [SerializeField] private IBattleEventProvider cameraFollowRestartEventProvider;
        
        
        protected override void EnteredTrigger()
        {
            
            Debug.Log("On Camera Entered Trigger");
            base.EnteredTrigger();
            _onEnterTrigger.OnNext(Unit.Default);

            cameraFollowRestartEventProvider?.OnAllEnemyDead.Subscribe(_ =>
            {
                ProCamera2D.Instance.FollowHorizontal = true;
            }).AddTo(this);
        }

        protected override void ExitedTrigger()
        {
        }
    }
}