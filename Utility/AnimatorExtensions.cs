using UniRx;
using UnityEngine;

namespace Utility
{
    public static class AnimatorExtensions{

        public static void SetTriggerOneFrame(this Animator self, string name)
        {
            self.SetTrigger(name);
            Observable
                .NextFrame()
                .Subscribe(_ => {}, () => {
                    // 1フレーム後のUpdate後にトリガーをリセットする
                    if (self != null) {
                        self.ResetTrigger(name);
                    }
                });
        }

        public static void SetTriggerOneFrame(this Animator self, int id)
        {
            self.SetTrigger(id);
            Observable
                .NextFrame()
                .Subscribe(_ => {}, () => {
                    // 1フレーム後のUpdate後にトリガーをリセットする
                    if (self != null) {
                        self.ResetTrigger(id);
                    }
                });
        }
    }
}