using System;

namespace ScreenOpRecorder.Application.Input
{
    public interface IInputEventListener : IDisposable
    {
        event Action<string>? KeyDown;

        event Action<int, int, bool>? MouseClicked;
    }
}
