using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using OpRec.Application.Recording.Ports;
using OpRec.Application.Recording.Session;
using OpRec.Application.Settings.Ports;
using OpRec.Application.System.Ports;

namespace OpRec.Application.Recording
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
            var path = _settingsService.Current.OutputDirPath;
            if (!string.IsNullOrWhiteSpace(path))
            {
                await _directoryOpenService.OpenAsync(path);
            }
        }
    }
}
