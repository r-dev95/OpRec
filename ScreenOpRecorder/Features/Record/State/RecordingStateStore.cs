using System;

using Windows.Foundation;

namespace ScreenOpRecorder.Features.Record.State
{
    public class RecordingStateStore : IRecordingStateStore
    {
        private readonly object _gate = new();
        private RecordingState _current = new(false, new Rect(0, 0, 0, 0), false, new Rect(0, 0, 0, 0));

        public RecordingState Current
        {
            get
            {
                lock (_gate)
                {
                    return _current;
                }
            }
        }

        public event Action<RecordingState>? StateChanged;

        public void SetSelection(Rect captureArea)
        {
            Update(_current with
            {
                HasSelection = true,
                CaptureArea = captureArea,
                ZoomArea = captureArea
            });
        }

        public void SetZoomArea(Rect zoomArea)
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
                CaptureArea = new Rect(0, 0, 0, 0),
                ZoomArea = new Rect(0, 0, 0, 0),
                IsRecording = false
            });
        }

        private void Update(RecordingState next)
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
