using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Common.Events;
using ScreenOpRecorder.Core.Recording.Events;
using ScreenOpRecorder.Core.Recording.Ports;
using ScreenOpRecorder.Core.Recording.State;
using ScreenOpRecorder.Core.Settings.Ports;
using ScreenOpRecorder.Domain.ValueObjects;

namespace ScreenOpRecorder.Core.Recording.UseCases
{
    public sealed class RecordingWorkflowService : IRecordingWorkflowService, IDisposable
    {
        private readonly ILogger<RecordingWorkflowService> _logger;
        private readonly IUserSettingsService _settingsService;
        private readonly IRecordingSessionStore _stateStore;
        private readonly IRecordingService _recordingEngine;
        private readonly IFolderOpenService _outputFolderOpener;
        private readonly IDisposable _zoomSubscription;

        public RecordingWorkflowService(
            ILogger<RecordingWorkflowService> logger,
            IUserSettingsService settingsService,
            IRecordingSessionStore stateStore,
            IRecordingService recordingEngine,
            IFolderOpenService outputFolderOpener,
            IEventBus eventBus)
        {
            _logger = logger;
            _settingsService = settingsService;
            _stateStore = stateStore;
            _recordingEngine = recordingEngine;
            _outputFolderOpener = outputFolderOpener;
            _zoomSubscription = eventBus.Subscribe<ZoomAreaChangedEvent>(OnZoomAreaChanged);
        }

        public bool SelectCaptureArea(ScreenRect captureArea)
        {
            if (_stateStore.Current.IsRecording)
            {
                return false;
            }

            var selected = _recordingEngine.TrySelectCaptureArea(captureArea);
            if (!selected)
            {
                return false;
            }

            _stateStore.SetSelection(captureArea);

            _logger.LogDebug("Selected Rect: {} x {} - {} x {}", captureArea.X, captureArea.Y, captureArea.Width, captureArea.Height);
            return true;
        }

        public async Task<bool> StartAsync()
        {
            if (!_stateStore.Current.HasSelection)
            {
                return false;
            }

            var started = await _recordingEngine.StartAsync();
            if (!started)
            {
                return false;
            }

            _stateStore.SetRecording(true);
            return true;
        }

        public async Task StopAsync()
        {
            await _recordingEngine.StopAsync();
            _stateStore.ClearSelection();

            if (_settingsService.Current.OpenOutputFolderAfterRecording)
            {
                await OpenOutputFolderAsync();
            }
        }

        public void Dispose()
        {
            _zoomSubscription.Dispose();
        }

        private void OnZoomAreaChanged(ZoomAreaChangedEvent evt)
        {
            _stateStore.SetZoomArea(evt.ZoomRect);
        }

        private async Task OpenOutputFolderAsync()
        {
            var path = _recordingEngine.LastOutputDirPath;
            if (!string.IsNullOrWhiteSpace(path))
            {
                await _outputFolderOpener.OpenAsync(path);
            }
        }
    }
}
