using System;
using System.Linq;
using MessagePipe;
using Sirenix.OdinInspector;
using UI;
using UniRx;
using UniRx.Diagnostics;
using UnityEngine;

namespace Characters.Inputs
{
    public class PlayerCommandEventProvider : SerializedMonoBehaviour, ICommandEventProvider
    {
        [SerializeField] IKeyInputEventProvider _keyInputEventProvider;
        
        
        public IObservable<InputCommand> OnCommandInput => _onCommandInput;
        private readonly Subject<InputCommand> _onCommandInput = new Subject<InputCommand>();
        
        // プレーヤー側は使わない
        public IReadOnlyReactiveProperty<Vector3> AnalogueMovementInput => _analogueMovementInput;
        public Vector3ReactiveProperty _analogueMovementInput = new Vector3ReactiveProperty();

        private Transform _playerTransform;

        private IPublisher<GlobalEventData> _publisher;

        private void Awake()
        {
            _playerTransform = GetComponent<Transform>();
        }

        // Start is called before the first frame update
        void Start()
        {
            _publisher =GlobalMessagePipe.GetPublisher<GlobalEventData>();
            
            CreateCommandObserver("1")
                .Do(x => MessageBroker.Default.Publish(x))
                .Subscribe(x =>
                {
                    _onCommandInput.OnNext(InputCommand.DownLeft);
                }).AddTo(this);
            CreateCommandObserver("2")
                .Do(x => MessageBroker.Default.Publish(x))
                .Subscribe(x =>
                {
                    _onCommandInput.OnNext(InputCommand.Down);
                }).AddTo(this);
            CreateCommandObserver("3")
                .Do(x => MessageBroker.Default.Publish(x))
                .Subscribe(x =>
                {
                    _onCommandInput.OnNext(InputCommand.DownRight);
                }).AddTo(this);
            CreateCommandObserver("4")
                .Do(x => MessageBroker.Default.Publish(x))
                .Subscribe(x =>
                {
                    _onCommandInput.OnNext(InputCommand.Left);
                }).AddTo(this);
            CreateCommandObserver("5")
                .Do(x => MessageBroker.Default.Publish(x))
                .Subscribe(x =>
                {
                    _onCommandInput.OnNext(InputCommand.Neutral);
                }).AddTo(this);
            CreateCommandObserver("6")
                .Do(x => MessageBroker.Default.Publish(x))
                .Subscribe(x =>
                {
                    _onCommandInput.OnNext(InputCommand.Right);
                }).AddTo(this);
            CreateCommandObserver("7")
                .Do(x => MessageBroker.Default.Publish(x))
                .Subscribe(x =>
                {
                    _onCommandInput.OnNext(InputCommand.UpLeft);
                }).AddTo(this);
            CreateCommandObserver("8")
                .Do(x => MessageBroker.Default.Publish(x))
                .Subscribe(x =>
                {
                    _onCommandInput.OnNext(InputCommand.Up);
                }).AddTo(this);
            CreateCommandObserver("9")
                .Do(x => MessageBroker.Default.Publish(x))
                .Subscribe(x =>
                {
                    _onCommandInput.OnNext(InputCommand.UpRight);
                }).AddTo(this);
            CreateCommandObserver("2A")
                .Do(x => Debug.Log("DownAttack"))
                .Subscribe(x =>
                {
                    _onCommandInput.OnNext(InputCommand.DownAttack);
                }).AddTo(this);
            
            
            CreateCommandObserver("A1")
                .Where(_ => IsFacingRight(_playerTransform))
                //.Do(_ => Debug.Log("Blocking"))
                //.ThrottleFirstFrame(30)
                .Subscribe(x =>
                {
                    _onCommandInput.OnNext(InputCommand.Block);
                }).AddTo(this);
            CreateCommandObserver("A4")
                .Where(_ => IsFacingRight(_playerTransform))
                //.Do(_ => Debug.Log("Blocking"))
                //.ThrottleFirstFrame(30)
                .Subscribe(x =>
                {
                    _onCommandInput.OnNext(InputCommand.Block);
                }).AddTo(this);
            CreateCommandObserver("A7")
                .Where(_ => IsFacingRight(_playerTransform))
                //.Do(_ => Debug.Log("Blocking"))
                //.ThrottleFirstFrame(30)
                .Subscribe(x =>
                {
                    _onCommandInput.OnNext(InputCommand.Block);
                }).AddTo(this);
            
            CreateCommandObserver("A3")
                .Where(_ => !IsFacingRight(_playerTransform))
                //.Do(_ => Debug.Log("Blocking"))
                //.ThrottleFirstFrame(30)
                .Subscribe(x =>
                {
                    _onCommandInput.OnNext(InputCommand.Block);
                }).AddTo(this);
            CreateCommandObserver("A6")
                .Where(_ => !IsFacingRight(_playerTransform))
                //.Do(_ => Debug.Log("Blocking"))
                //.ThrottleFirstFrame(30)
                .Subscribe(x =>
                {
                    _onCommandInput.OnNext(InputCommand.Block);
                }).AddTo(this);
            CreateCommandObserver("A9")
                .Where(_ => !IsFacingRight(_playerTransform))
                //.Do(_ => Debug.Log("Blocking"))
                //.ThrottleFirstFrame(30)
                .Subscribe(x =>
                {
                    _onCommandInput.OnNext(InputCommand.Block);
                }).AddTo(this);
            
            /*
            _keyInputEventProvider.OnKeyInput
                .BatchFrame()
                .Where(x => x.Contains("P"))
                */
            
            
            _keyInputEventProvider.OnKeyInput
                .Where(x => x.Key == 'M')
                .Pairwise()
                .Where(x => x.Current.IsDown != x.Previous.IsDown)
                .Select(x => x.Current)
                .Where(x => x.IsDown)
                .Do(x => MessageBroker.Default.Publish(x))
                .Subscribe(x =>
                {
                    Debug.Log($"{Time.frameCount}:megacrash1");
                    _onCommandInput.OnNext(InputCommand.MegaCrash);
                }).AddTo(this);
            

            // パンチ・ジャンプボタン.
            _keyInputEventProvider.OnKeyInput
                .Where(x => x.Key == 'A')
                .Pairwise()
                .Where(x => x.Current.IsDown != x.Previous.IsDown)
                .Select(x => x.Current)
                .Where(x => x.IsDown)
                .Do(x => MessageBroker.Default.Publish(x))
                .Subscribe(x =>
                {
                    _onCommandInput.OnNext(InputCommand.Attack);
                }).AddTo(this);
            
            _keyInputEventProvider.OnKeyInput
                .Where(x => x.Key == 'J')
                .Pairwise()
                .Where(x => x.Current.IsDown != x.Previous.IsDown)
                .Select(x => x.Current)
                .Where(x => x.IsDown)
                .Do(x => MessageBroker.Default.Publish(x))
                .Subscribe(x =>
                {
                    _onCommandInput.OnNext(InputCommand.Jump);
                }).AddTo(this);
            
            // パンチ離し.
            _keyInputEventProvider.OnKeyInput
                .Where(x => x.Key == 'A')
                .Pairwise()
                .Where(x => x.Current.IsDown != x.Previous.IsDown)
                .Select(x => x.Current)
                .Where(x => !x.IsDown)
                .Do(x => MessageBroker.Default.Publish(x))
                .Subscribe(x =>
                {
                    Debug.Log("Attack Release");
                    _onCommandInput.OnNext(InputCommand.AttackRelease);
                }).AddTo(this);

            // メニュー開く(今はリスタート)
            _keyInputEventProvider.OnKeyInput
                .Where(x => x.Key == 'O')
                .Pairwise()
                .Where(x => x.Current.IsDown != x.Previous.IsDown)
                .Select(x => x.Current)
                .Where(x => x.IsDown)
                .Subscribe(_ =>
                {
                    Debug.Log("Open menu button Pressed");
                    _publisher.Publish(GlobalEventData.RestartGame);
                }).AddTo(this);

            /*
            // 攻撃ボタン離しイベント.
            rewiredKeyInput.OnKeyInput
                .Where(k => k.Key == 'A' && !k.IsDown)
                .Do(x => MessageBroker.Default.Publish(x))
                .Subscribe(x =>
                {
                    _onCommandInput.OnNext(InputCommand.AttackRelease);
                }).AddTo(this);
            */

            /*
            CreateCommandObserver("236A")
                .Subscribe(x =>
                {
                    MessageBroker.Default.Publish("Hado-Ken!!");
                }).AddTo(this);
            */


        }

        private IObservable<KeyInfo> CreateCommandObserver(string command)
        {
            // ローカル関数.
            IObservable<KeyInfo> GetInputObserver(char c)
            {
                return _keyInputEventProvider.OnKeyInput.Where(k => k.Key == c && k.IsDown);
            }

            var observer = GetInputObserver(command[0]);

            for (var i = 1; i < command.Length; ++i)
            {
                var index = i;
                observer = observer
                    .Merge(GetInputObserver(command[index]))
                    .Buffer(2, 1)
                    .Where(x => x[1].Frame - x[0].Frame < 5)
                    .Where(x => x[0].Key == command[index - 1] && x[1].Key == command[index])
                    //.Do(x => Debug.Log($"Frame[0]:{x[0].Frame}, Frame[1]:{x[1].Frame}"))
                    .Select(x => x[1]);
            }

            return observer;
        }
        
        /*
        public IObservable<KeyInfo> CreateCommandObserver2(string command)
        {
            IObservable<KeyInfo> InputObserverSelector(char c)
                => rewiredKeyInput.OnKeyInput.Where(k => k.Key == c && k.IsDown);

            // 方向キーを識別するための関数
            bool IsDirection(char c) { return '1' <= c && c <= '9';}

            var observer = InputObserverSelector(command[0]);
            for (int i = 1; i < command.Length; ++i)
            {
                var index = i;
                // 同じ入力が連続する場合、マージしてしまうと通知が重複して飛んでくるのを回避
                if (command[index] != command[index - 1])
                {
                    observer = observer.Merge(InputObserverSelector(command[index]));
                }

                observer = observer
                    .Buffer(2, 1)
                    // 最初の入力がボタンの場合は間隔チェックが必要
                    .Where(b => (index == 1 && IsDirection(command[0]))
                                || b[1].Frame - b[0].Frame < 10)
                    .Where(b => b[0].Key == command[index - 1]
                                && b[1].Key == command[index])
                    .Select(b => b[1]);
            }

            return observer;
        }
        */

        private bool IsFacingRight(Transform trans)
        {
            return trans.forward.normalized == Vector3.right;
        }
    }
}