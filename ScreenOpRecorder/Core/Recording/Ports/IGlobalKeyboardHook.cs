using System;

namespace ScreenOpRecorder.Core.Recording.Ports
{
    public interface IGlobalKeyboardHook
    {
        event Action<string>? KeyDown;

        void Start();

        void Stop();
    }
}
