using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml;

using ScreenOpRecorder.Application.Recording.Session;
using ScreenOpRecorder.Common.Helpers;
using ScreenOpRecorder.Domain.Settings.ValueObjects;

using Windows.Foundation;

namespace ScreenOpRecorder.Presentation.Overlay.Guide
{
    public partial class MinimapState : ObservableObject
    {
        private double _scaleFactor = 1.0;
        private bool _enableMinimap;

        [ObservableProperty]
        public partial double Width { get; set; } = 150;

        [ObservableProperty]
        public partial double Height { get; set; }

        [ObservableProperty]
        public partial double ViewportX { get; set; }

        [ObservableProperty]
        public partial double ViewportY { get; set; }

        [ObservableProperty]
        public partial double ViewportWidth { get; set; }

        [ObservableProperty]
        public partial double ViewportHeight { get; set; }

        [ObservableProperty]
        public partial Visibility IsVisible { get; set; } = Visibility.Collapsed;

        public void SetScaleFactor(double scaleFactor)
        {
            _scaleFactor = scaleFactor;
        }

        public void ApplaySettings(UserSettings settings)
        {
            _enableMinimap = settings.EnableMinimap;
        }

        public void ApplySessionState(RecordingSessionState state)
        {
            if (!state.IsRecording || !_enableMinimap)
            {
                Reset();
                return;
            }

            var captureRect = state.HasSelection
                ? DpiHelper.ToLogical(
                    new Rect(state.CaptureArea.X, state.CaptureArea.Y, state.CaptureArea.Width, state.CaptureArea.Height),
                    _scaleFactor)
                : Rect.Empty;

            var viewportRect = state.IsRecording
                ? DpiHelper.ToLogical(
                    new Rect(state.ZoomArea.X, state.ZoomArea.Y, state.ZoomArea.Width, state.ZoomArea.Height),
                    _scaleFactor)
                : captureRect;

            Update(captureRect, viewportRect);
        }

        private void Update(Rect captureArea, Rect currentViewport)
        {
            // ズーム判定 (幅がキャプチャエリアより小さい場合)
            // 浮動小数点の誤差を考慮して少し余裕を持たせる
            bool isZoomed = currentViewport.Width < (captureArea.Width - 1.0);
            IsVisible = isZoomed ? Visibility.Visible : Visibility.Collapsed;

            if (!isZoomed || captureArea.Width == 0 || captureArea.Height == 0)
            {
                return;
            }

            double scale = Width / captureArea.Width;
            Height = captureArea.Height * scale;

            ViewportX = (currentViewport.X - captureArea.X) * scale;
            ViewportY = (currentViewport.Y - captureArea.Y) * scale;
            ViewportWidth = currentViewport.Width * scale;
            ViewportHeight = currentViewport.Height * scale;
        }

        private void Reset()
        {
            IsVisible = Visibility.Collapsed;
        }
    }
}
