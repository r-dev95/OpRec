using System;

namespace ScreenOpRecorder.Core.Input.Ports
{
    public interface IKeyboardHookService
    {
        event Action<string>? KeyDown;

        void Start();

        void Stop();
    }
}

