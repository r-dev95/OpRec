using System;

namespace ScreenOpRecorder.Core.Input
{
    public interface IInputHookUseCase : IDisposable
    {
        event Action<string>? KeyDown;

        event Action<int, int, bool>? MouseClicked;
    }
}
