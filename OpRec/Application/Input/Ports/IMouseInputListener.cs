using System;

namespace OpRec.Application.Input.Ports
{
    public interface IMouseInputListener
    {
        event Action<int, int, bool>? MouseClicked;

        void Start();

        void Stop();
    }
}

