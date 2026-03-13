using System;

namespace OpRec.Application.Events.Ports
{
    public interface IEventBus
    {
        IDisposable Subscribe<T>(Action<T> handler);

        void Publish<T>(T message);
    }
}

