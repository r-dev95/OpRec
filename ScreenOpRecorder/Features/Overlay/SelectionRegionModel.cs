using System;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml;

using Windows.Foundation;

namespace ScreenOpRecorder.Features.Overlay
{
    public partial class SelectionRegionModel : ObservableObject
    {
        [ObservableProperty]
        public partial double X { get; set; }

        [ObservableProperty]
        public partial double Y { get; set; }

        [ObservableProperty]
        public partial double Width { get; set; }

        partial void OnWidthChanged(double value) => OnPropertyChanged(nameof(SizeLabel));

        [ObservableProperty]
        public partial double Height { get; set; }

        partial void OnHeightChanged(double value) => OnPropertyChanged(nameof(SizeLabel));

        [ObservableProperty]
        public partial Rect CaptureAreaRect { get; set; }

        [ObservableProperty]
        public partial Visibility IsCaptureAreaVisible { get; set; } = Visibility.Collapsed;

        [ObservableProperty]
        public partial Visibility IsSizeTagVisible { get; set; } = Visibility.Collapsed;

        private Point _startPoint;
        private bool _isSelecting;

        public string SizeLabel => $"{(int)Width} x {(int)Height}";

        public bool HasSelection => Width > 0 && Height > 0;

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

            X = Math.Min(_startPoint.X, currentPoint.X);
            Y = Math.Min(_startPoint.Y, currentPoint.Y);
            Width = Math.Abs(_startPoint.X - currentPoint.X);
            Height = Math.Abs(_startPoint.Y - currentPoint.Y);
            CaptureAreaRect = new Rect(X, Y, Width, Height);
            return true;
        }

        public void EndSelection()
        {
            _isSelecting = false;
            IsSizeTagVisible = Visibility.Collapsed;
        }

        public Rect GetSelectionRect() => new(X, Y, Width, Height);

        public void ClearSelection()
        {
            IsCaptureAreaVisible = Visibility.Collapsed;
            IsSizeTagVisible = Visibility.Collapsed;
            CaptureAreaRect = new Rect(0, 0, 0, 0);
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
        }
    }
}
