using System;

using ScreenOpRecorder.Domain.ValueObjects;

namespace ScreenOpRecorder.Application.Recording.State
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
            Update(_current with
            {
                HasSelection = true,
                CaptureArea = captureArea,
                ZoomArea = captureArea
            });
        }

        public void SetZoomArea(ScreenRect zoomArea)
        {
            Update(_current with { ZoomArea = zoomArea });
        }

        public void SetRecording(bool isRecording)
        {
            Update(_current with { IsRecording = isRecording });
        }

        public void ClearSelection()
        {
            Update(_current with
            {
                HasSelection = false,
                CaptureArea = ScreenRect.Empty,
                ZoomArea = ScreenRect.Empty,
                IsRecording = false
            });
        }

        private void Update(RecordingSessionState next)
        {
            bool changed;
            lock (_gate)
            {
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
