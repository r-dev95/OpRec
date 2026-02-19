using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

using Windows.Foundation;

namespace ScreenOpRecorder.Features.Overlay
{
    public partial class InputFeedbackModel : ObservableObject
    {
        [ObservableProperty]
        public partial string CurrentKeyText { get; set; } = "";

        [ObservableProperty]
        public partial Rect KeyDisplayArea { get; set; }

        private CancellationTokenSource? _cts;
        private bool _isRecording;
        private Size _screenSize;

        public void SetScreenSize(double width, double height)
        {
            _screenSize = new Size(width, height);
            UpdateKeyDisplayArea(KeyDisplayArea);
        }

        public void SetRecordingState(bool isRecording, Rect captureAreaRect)
        {
            _isRecording = isRecording;
            if (_isRecording)
            {
                UpdateKeyDisplayArea(captureAreaRect);
                return;
            }

            CurrentKeyText = "";
            UpdateKeyDisplayArea(new Rect(0, 0, _screenSize.Width, _screenSize.Height));
        }

        public void SetZoomArea(Rect logicalZoomRect)
        {
            if (!_isRecording)
            {
                return;
            }

            UpdateKeyDisplayArea(logicalZoomRect);
        }

        public async Task ShowKeyAsync(string keyName)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            CurrentKeyText += keyName + " ";

            try
            {
                await Task.Delay(1500, _cts.Token);
                CurrentKeyText = "";
            }
            catch
            {
            }
        }

        private void UpdateKeyDisplayArea(Rect rect)
        {
            KeyDisplayArea = rect;
        }
    }
}
