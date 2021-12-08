using System;
using UniRx;
using UnityEngine;

namespace Characters.Inputs
{
    public class AIInputEventProvider : MonoBehaviour,IKeyInputEventProvider
    {
        public IObservable<KeyInfo> OnKeyInput => _onKeyInput;
        private readonly Subject<KeyInfo> _onKeyInput = new Subject<KeyInfo>();

        public void Move(Vector3 vec)
        {
            Debug.Log(vec);
            var key = ConvertVectorToChar(vec);

            var keyInfo = new KeyInfo {Key = key, IsDown = true, Frame = Time.frameCount};
            _onKeyInput.OnNext(keyInfo);
        }

        /// <summary>
        /// 方向入力のVectorをコマンドのCharに変換する(テンキー上の数字で方向を表す格ゲー方式)
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        private static char ConvertVectorToChar(Vector3 vec)
        {
            var key = '5';
            if (vec.x <= -0.1f && vec.z <= -0.1f) key = '1';
            else if (vec.x > -0.1f && vec.x < 0.1f && vec.z <= -0.1f) key = '2';
            else if (vec.x >= 0.1f && vec.z <= -0.1f) key = '3';

            else if (vec.x <= -0.1f && vec.z > -0.1f && vec.z < 0.1f) key = '4';
            else if (vec.x > -0.1f && vec.x < 0.1f && vec.z > -0.1f && vec.z < 0.1f) key = '5';
            else if (vec.x >= 0.1f && vec.z > -0.1f && vec.z < 0.1f) key = '6';

            else if (vec.x <= -0.1f && vec.z >= 0.1f) key = '7';
            else if (vec.x > -0.1f && vec.x < 0.1f && vec.z >= 0.1f) key = '8';
            else if (vec.x >= 0.1f && vec.z >= 0.1f) key = '9';
            return key;
        }

        public void NormalAttack()
        {
            var keyInfo = new KeyInfo {Key = 'A', IsDown = false, Frame = Time.frameCount};
            _onKeyInput.OnNext(keyInfo);
            keyInfo = new KeyInfo {Key = 'A', IsDown = true, Frame = Time.frameCount};
            _onKeyInput.OnNext(keyInfo);
        }

    }
}
