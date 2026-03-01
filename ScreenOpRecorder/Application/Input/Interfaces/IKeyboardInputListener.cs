using System;

namespace ScreenOpRecorder.Application.Input.Interfaces
{
    public interface IKeyboardInputListener
    {
        event Action<string>? KeyDown;

        void Start();

        void Stop();
    }
}

