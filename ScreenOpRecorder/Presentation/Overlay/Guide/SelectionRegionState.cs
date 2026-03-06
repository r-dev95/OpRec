using System;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml;

using ScreenOpRecorder.Application.Recording.Session;
using ScreenOpRecorder.Common.Helpers;

using Windows.Foundation;

namespace ScreenOpRecorder.Presentation.Overlay.Guide
{
    public partial class SelectionRegionState : ObservableObject
    {
        private double _scaleFactor = 1.0;
        private Point _startPoint;
        private bool _isSelecting;

        [ObservableProperty]
        public partial Rect CaptureAreaRect { get; set; }

        [ObservableProperty]
        public partial Visibility IsCaptureAreaVisible { get; set; } = Visibility.Collapsed;

        [ObservableProperty]
        public partial Visibility IsSizeTagVisible { get; set; } = Visibility.Collapsed;

        public string SizeLabel => $"{(int)CaptureAreaRect.Width} x {(int)CaptureAreaRect.Height}";
        partial void OnCaptureAreaRectChanged(Rect value) => OnPropertyChanged(nameof(SizeLabel));

        public bool HasSelection => CaptureAreaRect.Width > 0 && CaptureAreaRect.Height > 0;

        public void SetScaleFactor(double scaleFactor)
        {
            _scaleFactor = scaleFactor;
        }
        public void BeginSelection(Point startPoint)
        {
            _isSelecting = true;
            _startPoint = startPoint;
        }

        public bool UpdateSelection(Point currentPoint)
        {
            if (!_isSelecting)
            {
                return false;
            }

            IsCaptureAreaVisible = Visibility.Visible;
            IsSizeTagVisible = Visibility.Visible;

            var x = Math.Min(_startPoint.X, currentPoint.X);
            var y = Math.Min(_startPoint.Y, currentPoint.Y);
            var width = Math.Abs(_startPoint.X - currentPoint.X);
            var height = Math.Abs(_startPoint.Y - currentPoint.Y);
            CaptureAreaRect = new Rect(x, y, width, height);
            return true;
        }

        public void EndSelection()
        {
            _isSelecting = false;
            IsSizeTagVisible = Visibility.Collapsed;
        }

        public void ClearSelection()
        {
            IsCaptureAreaVisible = Visibility.Collapsed;
            IsSizeTagVisible = Visibility.Collapsed;
            CaptureAreaRect = new Rect(0, 0, 0, 0);
        }

        public void ApplySessionState(RecordingSessionState state)
        {
            UpdateCaptureAreaRect(state);
        }

        private void UpdateCaptureAreaRect(RecordingSessionState state)
        {
            var logicalCaptureRect = state.HasSelection
                ? DpiHelper.ToLogical(
                    new Rect(state.CaptureArea.X, state.CaptureArea.Y, state.CaptureArea.Width, state.CaptureArea.Height),
                    _scaleFactor)
                : Rect.Empty;

            logicalCaptureRect = state.IsRecording
                ? DpiHelper.ToLogical(
                    new Rect(state.ZoomArea.X, state.ZoomArea.Y, state.ZoomArea.Width, state.ZoomArea.Height),
                    _scaleFactor)
                : logicalCaptureRect;

            if (logicalCaptureRect != Rect.Empty)
            {
                CaptureAreaRect = logicalCaptureRect;
                IsCaptureAreaVisible = Visibility.Visible;
            }
        }
    }
}
