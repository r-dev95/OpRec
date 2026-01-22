using System;


namespace ScreenOpRecorder.Features.Input
{
    public interface IMouseHookService : IDisposable
    {
        event Action<int, int, bool>? MouseClicked;

        void Start();
    }
}
