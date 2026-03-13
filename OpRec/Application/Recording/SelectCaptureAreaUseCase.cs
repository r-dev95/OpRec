using Microsoft.Extensions.Logging;

using OpRec.Application.Recording.Ports;
using OpRec.Application.Recording.Session;
using OpRec.Domain.ValueObjects;

namespace OpRec.Application.Recording
{
    public sealed class SelectCaptureAreaUseCase : ISelectCaptureAreaUseCase
    {
        private readonly ILogger<SelectCaptureAreaUseCase> _logger;
        private readonly IRecordingSessionStore _stateStore;
        private readonly IRecordingService _recordingService;

        public SelectCaptureAreaUseCase(
            ILogger<SelectCaptureAreaUseCase> logger,
            IRecordingSessionStore stateStore,
            IRecordingService recordingService)
        {
            _logger = logger;
            _stateStore = stateStore;
            _recordingService = recordingService;
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
    }
}
