using System;

namespace ScreenOpRecorder.Application.Input.Interfaces
{
    public interface IMouseInputListener
    {
        event Action<int, int, bool>? MouseClicked;

        void Start();

        void Stop();
    }
}

