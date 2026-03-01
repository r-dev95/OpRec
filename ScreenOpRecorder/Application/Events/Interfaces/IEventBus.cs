using System;

namespace ScreenOpRecorder.Application.Events.Interfaces
{
    public interface IEventBus
    {
        IDisposable Subscribe<T>(Action<T> handler);

        void Publish<T>(T message);
    }
}

