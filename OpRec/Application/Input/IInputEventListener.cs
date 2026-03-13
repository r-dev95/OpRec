using System;

namespace OpRec.Application.Input
{
    public interface IInputEventListener : IDisposable
    {
        event Action<string>? KeyDown;

        event Action<int, int, bool>? MouseClicked;
    }
}
