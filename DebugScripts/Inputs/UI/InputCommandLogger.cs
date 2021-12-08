using System.Collections;
using System.Collections.Generic;
using Characters.Inputs;
using UniRx;
using UnityEngine;

public class InputCommandLogger : MonoBehaviour
{
    [SerializeField] private int maxRowCount = 6;

    public IReadOnlyReactiveCollection<string> InputCommandList => _inputCommandList;
    [SerializeField] ReactiveCollection<string> _inputCommandList;
    
    // Start is called before the first frame update
    void Awake()
    {
        _inputCommandList = new ReactiveCollection<string>();
        
        MessageBroker.Default.Receive<KeyInfo>()
            .Buffer(2,1)
            .Where(b => b[0].Key != b[1].Key)
            .Select(b => b[1])
            .Where(x => x.Key != '5') // ニュートラルは表示しない.
            .Subscribe(x =>
            {
                // 表示行数制限を超えたら一番古いものを消す.
                if(_inputCommandList.Count >= maxRowCount) _inputCommandList.RemoveAt(0);
            
                _inputCommandList.Add(x.ToStringWithEmoji());
            }).AddTo(this);
        
        MessageBroker.Default.Receive<string>()
            .Subscribe(x =>
            {
                // 表示行数制限を超えたら一番古いものを消す.
                if(_inputCommandList.Count >= maxRowCount) _inputCommandList.RemoveAt(0);
            
                _inputCommandList.Add(x);
            }).AddTo(this);
    }

}