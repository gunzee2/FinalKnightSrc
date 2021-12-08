using System;

namespace Characters.Inputs
{
        public interface IKeyInputEventProvider
        {
                public IObservable<KeyInfo> OnKeyInput { get; }
        }
}
