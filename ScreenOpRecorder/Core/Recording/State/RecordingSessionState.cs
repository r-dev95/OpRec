using ScreenOpRecorder.Domain.ValueObjects;

namespace ScreenOpRecorder.Core.Recording.State
{
    public record RecordingSessionState(bool HasSelection, ScreenRect CaptureArea, bool IsRecording, ScreenRect ZoomArea);
}
