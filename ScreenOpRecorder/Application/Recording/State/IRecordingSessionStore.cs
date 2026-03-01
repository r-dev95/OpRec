using System;

using ScreenOpRecorder.Domain.ValueObjects;

namespace ScreenOpRecorder.Application.Recording.State
{
    public interface IRecordingSessionStore
    {
        RecordingSessionState Current { get; }

        event Action<RecordingSessionState>? StateChanged;

        void SetSelection(ScreenRect captureArea);

        void SetZoomArea(ScreenRect zoomArea);

        void SetRecording(bool isRecording);

        void ClearSelection();
    }
}
