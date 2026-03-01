using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

using ScreenOpRecorder.Application.Events.Interfaces;
using ScreenOpRecorder.Application.Recording.Events;
using ScreenOpRecorder.Application.Recording.State;

namespace ScreenOpRecorder.Application.Recording
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
            _logger.LogDebug("RecordingSessionZoomSyncHostedService started.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _zoomSubscription?.Dispose();
            _zoomSubscription = null;
            _logger.LogDebug("RecordingSessionZoomSyncHostedService stopped.");
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
            _logger.LogDebug("Zoom area updated: {X}, {Y}, {Width}, {Height}",
                evt.ZoomRect.X,
                evt.ZoomRect.Y,
                evt.ZoomRect.Width,
                evt.ZoomRect.Height);
        }
    }
}
