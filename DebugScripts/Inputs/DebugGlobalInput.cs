using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugGlobalInput : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
            this.UpdateAsObservable()
                .Where(_ => Input.GetKeyDown(KeyCode.Alpha1))
                .Subscribe(_ =>
                    {
                        SceneManager.LoadScene (SceneManager.GetActiveScene().name);
                    }
                ).AddTo(this);
            this.UpdateAsObservable()
                .Where(_ => Input.GetKeyDown(KeyCode.Alpha2))
                .Subscribe(_ =>
                    {
                        EditorApplication.isPaused = !EditorApplication.isPaused;
                    }
                ).AddTo(this);
#endif
        
    }
}
