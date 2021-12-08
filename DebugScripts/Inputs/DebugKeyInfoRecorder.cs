using System;
using System.Collections.Generic;
using System.IO;
using Characters.Inputs;
using UniRx;
using UnityEngine;

namespace DebugScripts.Inputs
{
    public class DebugKeyInfoRecorder : MonoBehaviour
    {
        [SerializeField] private bool IsRecord;
        [SerializeField] private RewiredKeyInput rewiredKeyInput;

        private List<KeyInfo> _keyInfos = new List<KeyInfo>();

        private void Awake()
        {
            
        }

        // Start is called before the first frame update
        void Start()
        {
            var keyInput = rewiredKeyInput.GetComponent<IKeyInputEventProvider>();

            keyInput.OnKeyInput
                .Where(_ => IsRecord)
                .Subscribe(x =>
            {
                _keyInfos.Add(x);
            }).AddTo(this);

            MainThreadDispatcher.OnApplicationQuitAsObservable()
                .Where(_ => IsRecord)
                .Subscribe(_ =>
            {
                var dt = DateTime.Now.ToString("yyyyMMddHHmmss");
                var path = $"{Application.dataPath}/DebugData/KeyInfo/KeyInfo_{dt}.txt";

                using (var fs = new StreamWriter(path, true, System.Text.Encoding.GetEncoding("UTF-8")))
                {
                    foreach (var keyinfo in _keyInfos)
                    {
                        fs.WriteLine(keyinfo.ToSaveFileString());
                    }
                }
            }).AddTo(this);
        }
    }
}
