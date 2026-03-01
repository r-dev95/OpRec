using ScreenOpRecorder.Domain.ValueObjects;

namespace ScreenOpRecorder.Application.Recording.State
{
    public record RecordingSessionState(bool HasSelection, ScreenRect CaptureArea, bool IsRecording, ScreenRect ZoomArea);
}
