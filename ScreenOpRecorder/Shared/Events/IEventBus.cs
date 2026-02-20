using System;

namespace ScreenOpRecorder.Shared.Events
{
    public interface IEventBus
    {
        IDisposable Subscribe<T>(Action<T> handler);

        void Publish<T>(T message);
    }
}
