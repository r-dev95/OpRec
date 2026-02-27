using System;

namespace ScreenOpRecorder.Core.Input.Ports
{
    public interface IMouseHookService
    {
        event Action<int, int, bool>? MouseClicked;

        void Start();

        void Stop();
    }
}

