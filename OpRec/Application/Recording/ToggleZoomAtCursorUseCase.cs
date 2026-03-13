using Microsoft.Extensions.Logging;

using OpRec.Application.Recording.Ports;
using OpRec.Application.System.Ports;

namespace OpRec.Application.Recording
{
    public sealed class ToggleZoomAtCursorUseCase : IToggleZoomAtCursorUseCase
    {
        private readonly ILogger<ToggleZoomAtCursorUseCase> _logger;
        private readonly ICursorPositionService _cursorPositionService;
        private readonly IRecordingService _recordingService;

        public ToggleZoomAtCursorUseCase(
            ILogger<ToggleZoomAtCursorUseCase> logger,
            ICursorPositionService cursorPositionService,
            IRecordingService recordingService)
        {
            _logger = logger;
            _cursorPositionService = cursorPositionService;
            _recordingService = recordingService;
        }

        public bool TryToggle()
        {
            if (!_cursorPositionService.TryGetPosition(out var x, out var y))
            {
                _logger.LogDebug("Failed to get cursor position for zoom toggle.");
                return false;
            }

            return _recordingService.TryToggleZoomAt(x, y);
        }
    }
}
