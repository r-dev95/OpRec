using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Common.Events;
using ScreenOpRecorder.Core.Recording.Events;
using ScreenOpRecorder.Core.Recording.Ports;
using ScreenOpRecorder.Core.Recording.State;
using ScreenOpRecorder.Core.Settings.Ports;
using ScreenOpRecorder.Domain.ValueObjects;

namespace ScreenOpRecorder.Core.Recording
{
    public sealed class RecordingUseCase : IRecordingUseCase, IDisposable
    {
        private readonly ILogger<RecordingUseCase> _logger;
        private readonly IUserSettingsService _settingsService;
        private readonly IRecordingSessionStore _stateStore;
        private readonly IRecordingService _recordingService;
        private readonly IFolderOpenService _outputFolderOpener;
        private readonly IDisposable _zoomSubscription;

        public RecordingUseCase(
            ILogger<RecordingUseCase> logger,
            IUserSettingsService settingsService,
            IRecordingSessionStore stateStore,
            IRecordingService recordingService,
            IFolderOpenService outputFolderOpener,
            IEventBus eventBus)
        {
            _logger = logger;
            _settingsService = settingsService;
            _stateStore = stateStore;
            _recordingService = recordingService;
            _outputFolderOpener = outputFolderOpener;
            _zoomSubscription = eventBus.Subscribe<ZoomAreaChangedEvent>(OnZoomAreaChanged);
        }

        public bool SelectCaptureArea(ScreenRect captureArea)
        {
            if (!captureArea.HasArea || _stateStore.Current.IsRecording)
            {
                return false;
            }

            var selected = _recordingService.TrySelectCaptureArea(captureArea);
            if (!selected)
            {
                return false;
            }

            _stateStore.SetSelection(captureArea);

            _logger.LogDebug("Selected Rect: {X} x {Y} - {Width} x {Height}",
                captureArea.X, captureArea.Y, captureArea.Width, captureArea.Height);
            return true;
        }

        public async Task<bool> StartAsync()
        {
            if (!_stateStore.Current.HasSelection)
            {
                return false;
            }

            var started = await _recordingService.StartAsync();
            if (!started)
            {
                return false;
            }

            _stateStore.SetRecording(true);
            return true;
        }

        public async Task StopAsync()
        {
            await _recordingService.StopAsync();
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
            var path = _recordingService.LastOutputDirPath;
            if (!string.IsNullOrWhiteSpace(path))
            {
                await _outputFolderOpener.OpenAsync(path);
            }
        }
    }
}
