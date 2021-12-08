using System;
using Rewired;
using Sirenix.OdinInspector;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Characters.Inputs
{
    public class RewiredKeyInput : SerializedMonoBehaviour,IKeyInputEventProvider
    {
        public IObservable<KeyInfo> OnKeyInput => _onKeyInput;
        private readonly Subject<KeyInfo> _onKeyInput = new Subject<KeyInfo>();

        private Player _player;

        private bool _attackRepeatFlag = true;

        private void Awake()
        {
            _player = ReInput.players.GetPlayer(0);
        }

        private void Start()
        {
            this.UpdateAsObservable().Subscribe(x =>
            {
                InputAxisRaw(Time.frameCount);
                
                InputMultiButton("Attack", "Jump", Time.frameCount, 'M');
                InputMultiButtonRepeat("AttackRepeat", "Jump", Time.frameCount, 'M', _attackRepeatFlag);

                InputButton("Attack", Time.frameCount, 'A');
                InputButtonRepeat("AttackRepeat", Time.frameCount, 'A',ref _attackRepeatFlag);
                
                InputButton("Jump", Time.frameCount, 'J');
                
                
                InputButton("MegaCrash", Time.frameCount, 'M');
                
                InputButton("OpenMenu", Time.frameCount, 'O');


            }).AddTo(this);
            
            
        }

        private void InputAxisRaw(int frame)
        {
            var direction = new Vector2(_player.GetAxisRaw("HorizontalMovement"),
                _player.GetAxisRaw("VerticalMovement"));
            var prevDirection = new Vector2(_player.GetAxisRawPrev("HorizontalMovement"),
                _player.GetAxisRawPrev("VerticalMovement"));

            
            var value = 5;
            if (direction.x > 0.2f) value += 1;
            if (direction.x < -0.2f) value -= 1;
            if (direction.y > 0.2f) value += 3;
            if (direction.y < -0.2f) value -= 3;

            PublishKeyInput(value.ToString()[0], true, frame);
        }

        private void InputMultiButton(string actionName1, string actionName2, int frame, char key)
        {
            var currentButton1 = _player.GetButton(actionName1);
            var currentButton2 = _player.GetButton(actionName2);
            
            var prevButton1 = _player.GetButtonPrev(actionName1);
            var prevButton2 = _player.GetButtonPrev(actionName2);

            if (!currentButton1 || !currentButton2) return;
            
            // 既に一度同時押し済(前フレームが両方true)の場合は読み飛ばす(同時押しっぱなしで再発動しない).
            if (prevButton1 && prevButton2) return;

            PublishKeyInput( key, true, frame);
        }
        
        private void InputMultiButtonRepeat(string actionName1, string actionName2, int frame, char key, bool flag)
        {
            var currentButton1 = _player.GetButton(actionName1);
            var currentButton2 = _player.GetButton(actionName2);
            
            var prevButton1 = _player.GetButtonPrev(actionName1);
            var prevButton2 = _player.GetButtonPrev(actionName2);

            if (!currentButton1) return;
            if (!currentButton2) return;

            PublishKeyInput( key, flag, frame);
        }

        private void InputButton(string actionName, int frame, char key)
        {
            var current = _player.GetButton(actionName);
            var previous = _player.GetButtonPrev(actionName);
            PublishKeyInput( key, current, frame);

        }
        
        private void InputButtonRepeat(string actionName, int frame, char key,ref bool flag)
        {
            var current = _player.GetButton(actionName);

            // ボタン押下中だけ連打処理する.
            // 次のボタン連打開始に備えてflagはtrueにしておく.
            if (!current)
            {
                flag = true;
                return;
            }

            // 毎フレームtrue,falseを繰り返す.
            PublishKeyInput(key, flag, frame);

            flag= !flag;

        }

        private void PublishKeyInput(char key,  bool isDown, int frame)
        {
            var command = new KeyInfo
            {
                Key = key,
                IsDown = isDown,
                Frame = frame,
            };
            
            
            _onKeyInput.OnNext(command);
        }
    }
}