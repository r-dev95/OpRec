using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using ScreenOpRecorder.Application.Events.Ports;

namespace ScreenOpRecorder.Infrastructure.Events
{
    public class EventBus : IEventBus
    {
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Delegate, byte>> _handlers = new();

        public IDisposable Subscribe<T>(Action<T> handler)
        {
            var bucket = _handlers.GetOrAdd(typeof(T), _ => new ConcurrentDictionary<Delegate, byte>());
            bucket.TryAdd(handler, 0);

            return new Subscription(() => Unsubscribe(handler));
        }

        public void Publish<T>(T message)
        {
            if (!_handlers.TryGetValue(typeof(T), out var bucket) || bucket.IsEmpty)
            {
                return;
            }

            Exception? firstException = null;
            var snapshot = bucket.Keys.ToArray();

            foreach (var handler in snapshot)
            {
                try
                {
                    ((Action<T>)handler).Invoke(message);
                }
                catch (Exception ex)
                {
                    firstException ??= ex;
                }
            }

            if (firstException != null)
            {
                throw firstException;
            }
        }

        private void Unsubscribe<T>(Action<T> handler)
        {
            if (!_handlers.TryGetValue(typeof(T), out var bucket))
            {
                return;
            }

            bucket.TryRemove(handler, out _);

            if (bucket.IsEmpty)
            {
                _handlers.TryRemove(new KeyValuePair<Type, ConcurrentDictionary<Delegate, byte>>(typeof(T), bucket));
            }
        }

        private sealed class Subscription : IDisposable
        {
            private readonly Action _disposeAction;
            private int _disposed;

            public Subscription(Action disposeAction)
            {
                _disposeAction = disposeAction;
            }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) != 0)
                {
                    return;
                }

                _disposeAction();
            }
        }
    }
}

