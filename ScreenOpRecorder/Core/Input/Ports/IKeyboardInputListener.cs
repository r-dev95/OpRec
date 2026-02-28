using System;

namespace ScreenOpRecorder.Core.Input.Ports
{
    public interface IKeyboardInputListener
    {
        event Action<string>? KeyDown;

        void Start();

        void Stop();
    }
}

