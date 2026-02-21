using System;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml;

using ScreenOpRecorder.Features.Settings;

using Windows.Foundation;

namespace ScreenOpRecorder.Features.Overlay
{
    public partial class InputFeedbackState : ObservableObject
    {
        [ObservableProperty]
        public partial string CurrentKeyText { get; set; } = "";

        [ObservableProperty]
        public partial Rect KeyDisplayArea { get; set; }

        [ObservableProperty]
        public partial bool EnableKeyDisplay { get; set; } = true;

        [ObservableProperty]
        public partial HorizontalAlignment KeyHorizontalAlignment { get; set; } = HorizontalAlignment.Center;

        [ObservableProperty]
        public partial VerticalAlignment KeyVerticalAlignment { get; set; } = VerticalAlignment.Bottom;

        [ObservableProperty]
        public partial Thickness KeyMargin { get; set; } = new(0, 0, 0, 50);

        [ObservableProperty]
        public partial double KeyDisplayDurationSeconds { get; set; } = 1.5;

        public Visibility KeyVisibility => EnableKeyDisplay && !string.IsNullOrWhiteSpace(CurrentKeyText)
            ? Visibility.Visible
            : Visibility.Collapsed;
        partial void OnCurrentKeyTextChanged(string value) => OnPropertyChanged(nameof(KeyVisibility));
        partial void OnEnableKeyDisplayChanged(bool value) => OnPropertyChanged(nameof(KeyVisibility));

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
            if (!EnableKeyDisplay)
            {
                return;
            }

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            CurrentKeyText += keyName + " ";

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(KeyDisplayDurationSeconds), _cts.Token);
                CurrentKeyText = "";
            }
            catch
            {
            }
        }

        public void ApplySettings(UserSettings settings)
        {
            EnableKeyDisplay = settings.EnableKeyDisplay;
            KeyDisplayDurationSeconds = settings.KeyDisplayDurationSeconds;

            switch (settings.KeyDisplayPosition)
            {
                case KeyDisplayPosition.TopLeft:
                    KeyHorizontalAlignment = HorizontalAlignment.Left;
                    KeyVerticalAlignment = VerticalAlignment.Top;
                    KeyMargin = new Thickness(30, 50, 0, 0);
                    break;
                case KeyDisplayPosition.TopCenter:
                    KeyHorizontalAlignment = HorizontalAlignment.Center;
                    KeyVerticalAlignment = VerticalAlignment.Top;
                    KeyMargin = new Thickness(0, 50, 0, 0);
                    break;
                case KeyDisplayPosition.TopRight:
                    KeyHorizontalAlignment = HorizontalAlignment.Right;
                    KeyVerticalAlignment = VerticalAlignment.Top;
                    KeyMargin = new Thickness(0, 50, 30, 0);
                    break;
                case KeyDisplayPosition.BottomLeft:
                    KeyHorizontalAlignment = HorizontalAlignment.Left;
                    KeyVerticalAlignment = VerticalAlignment.Bottom;
                    KeyMargin = new Thickness(30, 0, 0, 50);
                    break;
                case KeyDisplayPosition.BottomRight:
                    KeyHorizontalAlignment = HorizontalAlignment.Right;
                    KeyVerticalAlignment = VerticalAlignment.Bottom;
                    KeyMargin = new Thickness(0, 0, 30, 50);
                    break;
                default:
                    KeyHorizontalAlignment = HorizontalAlignment.Center;
                    KeyVerticalAlignment = VerticalAlignment.Bottom;
                    KeyMargin = new Thickness(0, 0, 0, 50);
                    break;
            }
        }

        private void UpdateKeyDisplayArea(Rect rect)
        {
            KeyDisplayArea = rect;
        }
    }
}
