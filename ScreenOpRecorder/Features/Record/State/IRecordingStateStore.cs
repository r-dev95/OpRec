using System;

using Windows.Foundation;

namespace ScreenOpRecorder.Features.Record.State
{
    public interface IRecordingStateStore
    {
        RecordingState Current { get; }

        event Action<RecordingState>? StateChanged;

        void SetSelection(Rect captureArea);

        void SetZoomArea(Rect zoomArea);

        void SetRecording(bool isRecording);

        void ClearSelection();
    }
}
