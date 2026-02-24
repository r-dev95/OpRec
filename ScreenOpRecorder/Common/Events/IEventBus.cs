using System;

namespace ScreenOpRecorder.Common.Events
{
    public interface IEventBus
    {
        IDisposable Subscribe<T>(Action<T> handler);

        void Publish<T>(T message);
    }
}
