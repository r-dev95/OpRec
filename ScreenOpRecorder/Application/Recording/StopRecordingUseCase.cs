using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Application.Recording.Ports;
using ScreenOpRecorder.Application.Recording.Session;
using ScreenOpRecorder.Application.Settings.Ports;
using ScreenOpRecorder.Application.System.Ports;

namespace ScreenOpRecorder.Application.Recording
{
    public sealed class StopRecordingUseCase : IStopRecordingUseCase
    {
        private readonly ILogger<StopRecordingUseCase> _logger;
        private readonly IUserSettingsService _settingsService;
        private readonly IRecordingSessionStore _stateStore;
        private readonly IRecordingService _recordingService;
        private readonly IDirectoryOpenService _directoryOpenService;

        public StopRecordingUseCase(
            ILogger<StopRecordingUseCase> logger,
            IUserSettingsService settingsService,
            IRecordingSessionStore stateStore,
            IRecordingService recordingService,
            IDirectoryOpenService directoryOpenService)
        {
            _logger = logger;
            _settingsService = settingsService;
            _stateStore = stateStore;
            _recordingService = recordingService;
            _directoryOpenService = directoryOpenService;
        }

        public async Task StopAsync()
        {
            await _recordingService.StopAsync();
            _stateStore.ClearSelection();
            _logger.LogDebug("Recording stopped.");

            if (_settingsService.Current.OpenDirectoryAfterRecording)
            {
                await OpenDirectoryAsync();
            }
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
