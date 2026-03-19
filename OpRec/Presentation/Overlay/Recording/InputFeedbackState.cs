using System;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml;

using OpRec.Application.Recording.Session;
using OpRec.Common.Helpers;
using OpRec.Domain.Settings.Policies;
using OpRec.Domain.Settings.ValueObjects;

using Windows.Foundation;

namespace OpRec.Presentation.Overlay.Recording
{
    public partial class InputFeedbackState : ObservableObject
    {
        private double _scaleFactor = 1.0;
        private CancellationTokenSource? _cts;
        private Size _screenSize;

        [ObservableProperty]
        public partial string CurrentKeyText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial Rect KeyDisplayArea { get; set; }

        [ObservableProperty]
        public partial bool EnableKeyDisplay { get; set; } = UserSettingsDefaults.EnableKeyDisplay;

        [ObservableProperty]
        public partial HorizontalAlignment KeyHorizontalAlignment { get; set; } = HorizontalAlignment.Center;

        [ObservableProperty]
        public partial VerticalAlignment KeyVerticalAlignment { get; set; } = VerticalAlignment.Bottom;

        [ObservableProperty]
        public partial Thickness KeyMargin { get; set; } = new(0, 0, 0, 50);

        [ObservableProperty]
        public partial double KeyDisplayDurationSeconds { get; set; } = UserSettingsDefaults.KeyDisplayDurationSeconds;

        public Visibility KeyVisibility => EnableKeyDisplay && !string.IsNullOrWhiteSpace(CurrentKeyText)
            ? Visibility.Visible
            : Visibility.Collapsed;
        partial void OnCurrentKeyTextChanged(string value) => OnPropertyChanged(nameof(KeyVisibility));
        partial void OnEnableKeyDisplayChanged(bool value) => OnPropertyChanged(nameof(KeyVisibility));

        public void SetScaleFactor(double scaleFactor)
        {
            _scaleFactor = scaleFactor;
        }

        public void SetScreenSize(double width, double height)
        {
            _screenSize = new Size(width, height);
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
                case KeyDisplayPositionOptions.TopLeft:
                    KeyHorizontalAlignment = HorizontalAlignment.Left;
                    KeyVerticalAlignment = VerticalAlignment.Top;
                    KeyMargin = new Thickness(30, 50, 0, 0);
                    break;
                case KeyDisplayPositionOptions.TopCenter:
                    KeyHorizontalAlignment = HorizontalAlignment.Center;
                    KeyVerticalAlignment = VerticalAlignment.Top;
                    KeyMargin = new Thickness(0, 50, 0, 0);
                    break;
                case KeyDisplayPositionOptions.TopRight:
                    KeyHorizontalAlignment = HorizontalAlignment.Right;
                    KeyVerticalAlignment = VerticalAlignment.Top;
                    KeyMargin = new Thickness(0, 50, 30, 0);
                    break;
                case KeyDisplayPositionOptions.BottomLeft:
                    KeyHorizontalAlignment = HorizontalAlignment.Left;
                    KeyVerticalAlignment = VerticalAlignment.Bottom;
                    KeyMargin = new Thickness(30, 0, 0, 50);
                    break;
                case KeyDisplayPositionOptions.BottomRight:
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

        public void ApplySessionState(RecordingSessionState state)
        {
            UpdateKeyDisplayArea(state);
        }

        private void UpdateKeyDisplayArea(RecordingSessionState state)
        {
            var logicalCaptureRect = state.HasSelection
                ? DpiHelper.ToLogical(
                    new Rect(state.CaptureArea.X, state.CaptureArea.Y, state.CaptureArea.Width, state.CaptureArea.Height),
                    _scaleFactor)
                : new Rect(0, 0, _screenSize.Width, _screenSize.Height);

            logicalCaptureRect = state.IsRecording
                ? DpiHelper.ToLogical(
                    new Rect(state.ZoomArea.X, state.ZoomArea.Y, state.ZoomArea.Width, state.ZoomArea.Height),
                    _scaleFactor)
                : logicalCaptureRect;

            KeyDisplayArea = logicalCaptureRect;
        }
    }
}
