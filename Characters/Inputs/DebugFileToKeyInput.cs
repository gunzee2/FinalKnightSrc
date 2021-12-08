using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Characters.Inputs
{
    public class DebugFileToKeyInput : MonoBehaviour,IKeyInputEventProvider
    {
        public IObservable<KeyInfo> OnKeyInput => _onKeyInput;
        private readonly Subject<KeyInfo> _onKeyInput = new Subject<KeyInfo>();

        [SerializeField] private TextAsset keyInputText;

        [SerializeField] private bool isLoop;


        private void Start()
        {
            var keyInfos = ConvertTextToKeyInfoList(keyInputText.text);
            var groupList = keyInfos.GroupBy(x => x.Frame)
                .Select(x => new List<KeyInfo>(x)).ToList();

            var index = 0;

            this.UpdateAsObservable().Subscribe(_ =>
            {
                if (index >= groupList.Count())
                {
                    if (isLoop) index = 0;
                    else return;
                }

                foreach (var group in groupList[index])
                {
                    _onKeyInput.OnNext(group);
                        
                }
                index++;

            }).AddTo(this);

        }


        private List<KeyInfo> ConvertTextToKeyInfoList(string text)
        {
            var listText = text.Split('\n');

            var keyinfos = listText
                .TakeWhile(t => t != "")
                .Select(KeyInfo.ConvertTextToKeyInfo)
                .ToList();

            return keyinfos;
        }

    }
}