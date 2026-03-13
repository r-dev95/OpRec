using OpRec.Domain.ValueObjects;

namespace OpRec.Application.Recording.Session
{
    public record RecordingSessionState(bool HasSelection, ScreenRect CaptureArea, bool IsRecording, ScreenRect ZoomArea);
}
