using Windows.Foundation;

namespace ScreenOpRecorder.Features.Record.State
{
    public record RecordingState(bool HasSelection, Rect CaptureArea, bool IsRecording, Rect ZoomArea);
}
