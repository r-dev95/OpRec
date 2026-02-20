using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Features.Record.Events;
using ScreenOpRecorder.Features.Record.State;
using ScreenOpRecorder.Shared.Events;
using ScreenOpRecorder.Shared.Helpers;

using Windows.Foundation;
using Windows.Graphics.Capture;

namespace ScreenOpRecorder.Features.Record
{
    public class RecordingDomainService : IRecordingDomainService, IDisposable
    {
        private readonly ILogger<RecordingDomainService> _logger;
        private readonly RecordService _recordService;
        private readonly IRecordingStateStore _stateStore;
        private readonly IDisposable _zoomSubscription;

        private GraphicsCaptureItem? _captureItem;
        private Rect _captureArea;

        public RecordingDomainService(ILogger<RecordingDomainService> logger, RecordService recordService, IRecordingStateStore stateStore, IEventBus eventBus)
        {
            _logger = logger;
            _recordService = recordService;
            _stateStore = stateStore;
            _zoomSubscription = eventBus.Subscribe<ZoomAreaChangedEvent>(OnZoomAreaChanged);
        }

        public bool SelectCaptureArea(Rect captureArea)
        {
            if (_stateStore.Current.IsRecording)
            {
                return false;
            }

            var captureItem = WindowHelper.CreateForMonitor(captureArea.X, captureArea.Y, captureArea.Width, captureArea.Height);
            if (captureItem == null)
            {
                return false;
            }

            _captureArea = captureArea;
            _captureItem = captureItem;
            _stateStore.SetSelection(captureArea);

            _logger.LogDebug("selectedRect: {} x {} - {} x {}", captureArea.X, captureArea.Y, captureArea.Width, captureArea.Height);
            _logger.LogDebug("selected item: {}, {} x {}", _captureItem.DisplayName, _captureItem.Size.Width, _captureItem.Size.Height);
            return true;
        }

        public async Task<bool> StartAsync()
        {
            if (_captureItem == null || !_stateStore.Current.HasSelection)
            {
                return false;
            }

            await _recordService.StartAsync(_captureItem, _captureArea);
            _stateStore.SetRecording(true);
            return true;
        }

        public async Task StopAsync()
        {
            await _recordService.StopAsync();
            _stateStore.ClearSelection();
            _captureItem = null;
        }

        public void Dispose()
        {
            _zoomSubscription.Dispose();
        }

        private void OnZoomAreaChanged(ZoomAreaChangedEvent evt)
        {
            _stateStore.SetZoomArea(evt.ZoomRect);
        }
    }
}
