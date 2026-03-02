using System;

using ScreenOpRecorder.Domain.ValueObjects;

namespace ScreenOpRecorder.Application.Recording.Session
{
    public class RecordingSessionStore : IRecordingSessionStore
    {
        private readonly object _gate = new();
        private RecordingSessionState _current = new(false, ScreenRect.Empty, false, ScreenRect.Empty);

        public RecordingSessionState Current
        {
            get
            {
                lock (_gate)
                {
                    return _current;
                }
            }
        }

        public event Action<RecordingSessionState>? StateChanged;

        public void SetSelection(ScreenRect captureArea)
        {
            Update(current => current with
            {
                HasSelection = true,
                CaptureArea = captureArea,
                ZoomArea = captureArea
            });
        }

        public void SetZoomArea(ScreenRect zoomArea)
        {
            Update(current => current with { ZoomArea = zoomArea });
        }

        public void SetRecording(bool isRecording)
        {
            Update(current => current with { IsRecording = isRecording });
        }

        public void ClearSelection()
        {
            Update(current => current with
            {
                HasSelection = false,
                CaptureArea = ScreenRect.Empty,
                ZoomArea = ScreenRect.Empty,
                IsRecording = false
            });
        }

        private void Update(Func<RecordingSessionState, RecordingSessionState> updater)
        {
            bool changed;
            RecordingSessionState next;
            lock (_gate)
            {
                next = updater(_current);
                changed = _current != next;
                _current = next;
            }

            if (changed)
            {
                StateChanged?.Invoke(next);
            }
        }
    }
}
