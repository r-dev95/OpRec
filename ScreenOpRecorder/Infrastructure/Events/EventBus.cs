using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ScreenOpRecorder.Application.Events.Interfaces;

namespace ScreenOpRecorder.Infrastructure.Events
{
    public class EventBus : IEventBus
    {
        private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
        private readonly object _gate = new();

        public IDisposable Subscribe<T>(Action<T> handler)
        {
            lock (_gate)
            {
                if (!_handlers.TryGetValue(typeof(T), out var list))
                {
                    list = new List<Delegate>();
                    _handlers[typeof(T)] = list;
                }
                list.Add(handler);
            }

            return new Subscription(() => Unsubscribe(handler));
        }

        public void Publish<T>(T message)
        {
            Delegate[] snapshot;
            lock (_gate)
            {
                if (!_handlers.TryGetValue(typeof(T), out var list) || list.Count == 0)
                {
                    return;
                }
                snapshot = list.ToArray();
            }

            foreach (var handler in snapshot)
            {
                ((Action<T>)handler).Invoke(message);
            }
        }

        private void Unsubscribe<T>(Action<T> handler)
        {
            lock (_gate)
            {
                if (!_handlers.TryGetValue(typeof(T), out var list))
                {
                    return;
                }

                list.Remove(handler);
                if (list.Count == 0)
                {
                    _handlers.TryRemove(typeof(T), out _);
                }
            }
        }

        private sealed class Subscription : IDisposable
        {
            private readonly Action _disposeAction;
            private bool _disposed;

            public Subscription(Action disposeAction)
            {
                _disposeAction = disposeAction;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposeAction();
                _disposed = true;
            }
        }
    }
}

