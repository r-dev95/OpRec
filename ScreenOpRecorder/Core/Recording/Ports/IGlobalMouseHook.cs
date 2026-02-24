using System;

namespace ScreenOpRecorder.Core.Recording.Ports
{
    public interface IGlobalMouseHook
    {
        event Action<int, int, bool>? MouseClicked;

        void Start();

        void Stop();
    }
}
