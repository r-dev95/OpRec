using CommunityToolkit.Mvvm.ComponentModel;
using Windows.Foundation;
using Microsoft.UI.Xaml;

namespace ScreenOpRecorder.Features.Overlay
{
    public partial class MinimapViewModel : ObservableObject
    {
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

        public void Update(Rect captureArea, Rect currentViewport)
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

        public void Reset()
        {
            IsVisible = Visibility.Collapsed;
        }
    }
}
