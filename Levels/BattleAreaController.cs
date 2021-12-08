using System;
using System.Collections.Generic;
using System.Linq;
using Cameras;
using Characters;
using Characters.Actions;
using Com.LuisPedroFonseca.ProCamera2D;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Levels
{
    public class BattleAreaController : SerializedMonoBehaviour,IBattleEventProvider
    {
        [SerializeField] private List<GameObject> enemyPrefabs;
        [SerializeField] private List<Transform> spawnPoint;

        [SerializeField] private List<GameObject> initialEnemyInstances;

        [SerializeField] private float enemySpawnDelay;
        [SerializeField] private float enableCameraMoveDelay;

        [SerializeField] private IBattleEventProvider parentBattleEventProvider;
        
        
        
        [ShowInInspector] private ReactiveCollection<GameObject> _enemyInstances;

        
        private IBattleAreaTriggerEventProvider _battleAreaTriggerEventProvider;

        public IObservable<Unit> OnAllEnemyDead => _onAllEnemyDead;
        private readonly Subject<Unit> _onAllEnemyDead = new Subject<Unit>();
    
        // Start is called before the first frame update
        void Start()
        {
            _battleAreaTriggerEventProvider = GetComponent<IBattleAreaTriggerEventProvider>();
            
            if (parentBattleEventProvider != null)
            {
                parentBattleEventProvider.OnAllEnemyDead.Subscribe(_ =>
                {
                    StartBattleEvent();
                }).AddTo(this);
            }
            else
            {
                _battleAreaTriggerEventProvider.OnEnterTrigger.Subscribe(_ =>
                {
                    StartBattleEvent();
                    ProCamera2D.Instance.FollowHorizontal = false;
                }).AddTo(this);

            }
        }
    
        private void StartBattleEvent(){
            _enemyInstances = new ReactiveCollection<GameObject>(initialEnemyInstances);
            

            Observable.Timer(TimeSpan.FromSeconds(enemySpawnDelay)).Subscribe(_ =>
            {
                for (var i = 0; i < enemyPrefabs.Count; i++)
                {
                    var go = Instantiate(enemyPrefabs[i], spawnPoint[i].position, spawnPoint[i].rotation);
                    go.GetComponent<EnemyBehaviorController>().EnableBehavior();
                    go.GetComponent<ICharacterStateProvider>().CurrentActionState
                        .Where(x => x == ActionState.Dead)
                        .Subscribe(_ =>
                        {
                            _enemyInstances.Remove(go);
                        }).AddTo(go);
                    _enemyInstances.Add(go);
                }
                    
            }).AddTo(this);

            // もしこのイベントが実行される前に立っている敵が倒された場合、削除してリストを作成し直す
            var enemies = _enemyInstances.ToList();
            enemies.RemoveAll(x => x == null);
            enemies.RemoveAll(x => x.GetComponent<ICharacterStateProvider>().CurrentActionState.Value == ActionState.Dead);
            
            _enemyInstances = new ReactiveCollection<GameObject>(enemies);

            foreach (var instance in _enemyInstances)
            {
                Debug.Log(instance);
                instance.GetComponent<ICharacterStateProvider>().CurrentActionState
                    .Where(x => x == ActionState.Dead)
                    .Subscribe(_ =>
                    {
                        _enemyInstances.Remove(instance);
                    }).AddTo(instance);
            }
            
            _enemyInstances.ObserveCountChanged().Where(x => x <= 0).Subscribe(_ =>
            {
                Observable.Timer(TimeSpan.FromSeconds(enableCameraMoveDelay)).Subscribe(_ =>
                {
                    _onAllEnemyDead.OnNext(Unit.Default);
                }).AddTo(this);
            }).AddTo(this);

            // もしイベント開始直後に全員居なかった場合はすぐ全員撃破イベントを実行する
            if (_enemyInstances.Count <= 0 && enemyPrefabs.Count <= 0)
            {
                _onAllEnemyDead.OnNext(Unit.Default);
            }
        }
    
    }
    
}