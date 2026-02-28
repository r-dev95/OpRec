using System;

namespace ScreenOpRecorder.Core.Input
{
    public interface IInputEventListener : IDisposable
    {
        event Action<string>? KeyDown;

        event Action<int, int, bool>? MouseClicked;
    }
}
