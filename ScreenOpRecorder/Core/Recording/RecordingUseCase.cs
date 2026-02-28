using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Core.Events.Interfaces;
using ScreenOpRecorder.Core.Recording.Events;
using ScreenOpRecorder.Core.Recording.Interfaces;
using ScreenOpRecorder.Core.Recording.State;
using ScreenOpRecorder.Core.Settings.Interfaces;
using ScreenOpRecorder.Core.System.Interfaces;
using ScreenOpRecorder.Domain.ValueObjects;

namespace ScreenOpRecorder.Core.Recording
{
    public sealed class RecordingUseCase : IRecordingUseCase, IDisposable
    {
        private readonly ILogger<RecordingUseCase> _logger;
        private readonly IUserSettingsService _settingsService;
        private readonly IRecordingSessionStore _stateStore;
        private readonly IRecordingService _recordingService;
        private readonly IDirectoryOpenService _directoryOpenService;
        private readonly IDisposable _zoomSubscription;

        public RecordingUseCase(
            ILogger<RecordingUseCase> logger,
            IUserSettingsService settingsService,
            IRecordingSessionStore stateStore,
            IRecordingService recordingService,
            IDirectoryOpenService directoryOpenService,
            IEventBus eventBus)
        {
            _logger = logger;
            _settingsService = settingsService;
            _stateStore = stateStore;
            _recordingService = recordingService;
            _directoryOpenService = directoryOpenService;
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

            if (_settingsService.Current.OpenDirectoryAfterRecording)
            {
                await OpenDirectoryAsync();
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

        private async Task OpenDirectoryAsync()
        {
            var path = _recordingService.LastOutputDirPath;
            if (!string.IsNullOrWhiteSpace(path))
            {
                await _directoryOpenService.OpenAsync(path);
            }
        }
    }
}

