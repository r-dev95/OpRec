using ScreenOpRecorder.Domain.ValueObjects;

namespace ScreenOpRecorder.Application.Recording.Session
{
    public record RecordingSessionState(bool HasSelection, ScreenRect CaptureArea, bool IsRecording, ScreenRect ZoomArea);
}
