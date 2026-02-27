using System;

namespace ScreenOpRecorder.Core.Input
{
    public interface IInputHookService : IDisposable
    {
        event Action<string>? KeyDown;

        event Action<int, int, bool>? MouseClicked;
    }
}
