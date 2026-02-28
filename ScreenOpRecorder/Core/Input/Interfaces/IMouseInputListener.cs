using System;

namespace ScreenOpRecorder.Core.Input.Interfaces
{
    public interface IMouseInputListener
    {
        event Action<int, int, bool>? MouseClicked;

        void Start();

        void Stop();
    }
}

