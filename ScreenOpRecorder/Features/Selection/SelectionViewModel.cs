using System;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml;

using Windows.Foundation;

namespace ScreenOpRecorder.Features.Selection
{
    public partial class SelectionViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial double X
        {
            get; set;
        }

        [ObservableProperty]
        public partial double Y
        {
            get; set;
        }

        [ObservableProperty]
        public partial double Width
        {
            get; set;
        }

        partial void OnWidthChanged(double value) => OnPropertyChanged(nameof(SizeLabel));

        [ObservableProperty]
        public partial double Height
        {
            get; set;
        }

        partial void OnHeightChanged(double value) => OnPropertyChanged(nameof(SizeLabel));

        [ObservableProperty]
        public partial Visibility IsSizeTagVisible
        {
            get; set;
        }

        // 表示用の文字列（Width x Height）
        public string SizeLabel => $"{(int)Width} x {(int)Height}";

        public SelectionViewModel()
        {
            IsSizeTagVisible = Visibility.Collapsed;
        }

        public void SetCaptureRect(Point startPoint, Point currentPoint)
        {
            X = Math.Min(startPoint.X, currentPoint.X);
            Y = Math.Min(startPoint.Y, currentPoint.Y);
            Width = Math.Abs(startPoint.X - currentPoint.X);
            Height = Math.Abs(startPoint.Y - currentPoint.Y);
        }

        public Rect GetCaptureRect() => new Rect(X, Y, Width, Height);
    }
}
