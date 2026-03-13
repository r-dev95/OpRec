using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OpRec.Application.Events.Ports;
using OpRec.Application.Recording.Events;

namespace OpRec.Application.Recording.Session
{
    public sealed class RecordingSessionZoomSyncHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<RecordingSessionZoomSyncHostedService> _logger;
        private readonly IRecordingSessionStore _stateStore;
        private readonly IEventBus _eventBus;
        private IDisposable? _zoomSubscription;

        public RecordingSessionZoomSyncHostedService(
            ILogger<RecordingSessionZoomSyncHostedService> logger,
            IRecordingSessionStore stateStore,
            IEventBus eventBus)
        {
            _logger = logger;
            _stateStore = stateStore;
            _eventBus = eventBus;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _zoomSubscription = _eventBus.Subscribe<ZoomAreaChangedEvent>(OnZoomAreaChanged);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _zoomSubscription?.Dispose();
            _zoomSubscription = null;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _zoomSubscription?.Dispose();
            _zoomSubscription = null;
        }

        private void OnZoomAreaChanged(ZoomAreaChangedEvent evt)
        {
            _stateStore.SetZoomArea(evt.ZoomRect);
        }
    }
}
