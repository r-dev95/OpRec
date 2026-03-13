using System;

namespace ScreenOpRecorder.Application.Input.Ports
{
    public interface IKeyboardInputListener
    {
        event Action<string>? KeyDown;

        void Start();

        void Stop();
    }
}

