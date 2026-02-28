using System;

namespace ScreenOpRecorder.Core.Input.Interfaces
{
    public interface IKeyboardInputListener
    {
        event Action<string>? KeyDown;

        void Start();

        void Stop();
    }
}

