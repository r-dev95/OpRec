using System;

namespace ScreenOpRecorder.Core.Recording.Ports
{
    public interface IKeyboardHookService
    {
        event Action<string>? KeyDown;

        void Start();

        void Stop();
    }
}
