using System;


namespace ScreenOpRecorder.Features.Input
{
    public interface IKeyboardHookService : IDisposable
    {
        event Action<string>? KeyDown;

        void Start();
    }
}
