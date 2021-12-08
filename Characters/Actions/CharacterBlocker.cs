using System;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Characters.Actions
{
    public class CharacterBlocker : SerializedMonoBehaviour,IBlockEventProvider
    {
        [SerializeField] private int BlockingFrame;
        
        private ICharacterStateProvider _characterStateProvider;
        private IMoveEventProvider _moveEventProvider;

        public IReadOnlyReactiveProperty<bool> OnBlocking => _onBlocking;
        private BoolReactiveProperty _onBlocking = new BoolReactiveProperty(false);

        
        private void Awake()
        {
            _characterStateProvider = GetComponent<ICharacterStateProvider>();
            _moveEventProvider = GetComponent<IMoveEventProvider>();
        }

        private void Start()
        {
            // 移動開始した瞬間をキャッチするイベント
            _moveEventProvider
                .IsMoving
                .Pairwise()
                .Where(x => x.Previous == false && x.Current == true)
                .Subscribe(_ =>
                {
                    Debug.Log("is moving", gameObject);
                    _onBlocking.Value = true;
                }).AddTo(this);
            // 方向転換した瞬間
            _moveEventProvider
                .OnRotate
                .Subscribe(_ =>
                {
                    Debug.Log("on Rotate", gameObject);
                    _onBlocking.Value = true;
                }).AddTo(this);
            
            // 指定フレーム経過後にフラグをOFFにする.
            _onBlocking
                .Pairwise()
                .Where(x => x.Previous == false && x.Current == true)
                .DelayFrame(BlockingFrame)
                .Subscribe(_ =>
                {
                    _onBlocking.Value = false;
                }).AddTo(this);

            // 空中に居てもOFFにする
            _characterStateProvider
                .IsGrounded
                .Where(x => x == false)
                .Where(_ => _onBlocking.Value == true)
                .Subscribe(_ =>
                {
                    _onBlocking.Value = false;
                }).AddTo(this);
        }
    }
}
