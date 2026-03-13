using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using OpRec.Application.Recording.Ports;
using OpRec.Application.Recording.Session;

namespace OpRec.Application.Recording
{
    public sealed class StartRecordingUseCase : IStartRecordingUseCase
    {
        private readonly ILogger<StartRecordingUseCase> _logger;
        private readonly IRecordingSessionStore _stateStore;
        private readonly IRecordingService _recordingService;

        public StartRecordingUseCase(
            ILogger<StartRecordingUseCase> logger,
            IRecordingSessionStore stateStore,
            IRecordingService recordingService)
        {
            _logger = logger;
            _stateStore = stateStore;
            _recordingService = recordingService;
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
            _logger.LogDebug("Recording started.");
            return true;
        }
    }
}
