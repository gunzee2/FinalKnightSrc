using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;

public class InputCommandLogProvider : MonoBehaviour
{
    [SerializeField] private TMP_Text logText;

    [SerializeField] private InputCommandLogger _commandLogger;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        _commandLogger.InputCommandList
            .ObserveAdd()
            .Subscribe(x =>
            {
                var commandStr = _commandLogger.InputCommandList
                    .Aggregate("", (current, command) => current + (command + "\n"));

                logText.text = commandStr;
            }).AddTo(this);
    }
}
