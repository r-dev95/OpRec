using System;

using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ScreenOpRecorder.Features.Overlay
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OverlayPage : Page
    {
        private readonly ILogger _logger;
        private readonly OverlayViewModel ViewModel;
        public OverlayPage(ILogger<OverlayPage> logger, MainWindow mainWindow, OverlayViewModel viewModel)
        {
            InitializeComponent();
            _logger = logger;
            ViewModel = viewModel;

            mainWindow.ExtendsContentIntoTitleBar = true;
            OverlayHelper.SetAlwaysOnTop(mainWindow, true);
            OverlayHelper.SetClickThrough(mainWindow, true);
            OverlayHelper.SetWindowOpacity(mainWindow, 128);
            OverlayHelper.MaximizeWindow(mainWindow);

            var scale = OverlayHelper.GetScaleFactor(mainWindow);
            ViewModel.Initialize(scale);

            ViewModel.RippleRequested += OnRippleRequested;
            
        }

        private void OnRippleRequested(double x, double y, bool isDouble)
        {
            // Hookイベントは別スレッドから来るため、UIスレッドへディスパッチ
            DispatcherQueue.TryEnqueue(() =>
            {
                ShowRipple(x, y, isDouble);
            });
        }

        private void ShowRipple(double x, double y, bool isDouble)
        {
            const int width = 20;
            const int height = 20;
            const int duration = 500;

            var ripple = new Ellipse
            {
                Width = width,
                Height = height,
                Stroke = new SolidColorBrush(isDouble ? Colors.OrangeRed : Colors.Cyan),
                StrokeThickness = 3,
                RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5),
                RenderTransform = new ScaleTransform { ScaleX = 1, ScaleY = 1 }
            };

            Canvas.SetLeft(ripple, x - 0.5 * width);
            Canvas.SetTop(ripple, y - 0.5 * height);
            OverlayCanvas.Children.Add(ripple);

            // アニメーションの定義
            var sb = new Storyboard();

            // スケール拡大
            var scaleXAnimation = new DoubleAnimation { To = 3, Duration = TimeSpan.FromMilliseconds(duration) };
            Storyboard.SetTarget(scaleXAnimation, ripple.RenderTransform);
            Storyboard.SetTargetProperty(scaleXAnimation, "ScaleX");

            var scaleYAnimation = new DoubleAnimation { To = 3, Duration = TimeSpan.FromMilliseconds(duration) };
            Storyboard.SetTarget(scaleYAnimation, ripple.RenderTransform);
            Storyboard.SetTargetProperty(scaleYAnimation, "ScaleY");

            // 不透明度を下げる（フェードアウト）
            var opacityAnimation = new DoubleAnimation { To = 0, Duration = TimeSpan.FromMilliseconds(duration) };
            Storyboard.SetTarget(opacityAnimation, ripple);
            Storyboard.SetTargetProperty(opacityAnimation, "Opacity");

            sb.Children.Add(scaleXAnimation);
            sb.Children.Add(scaleYAnimation);
            sb.Children.Add(opacityAnimation);

            sb.Completed += (s, e) => OverlayCanvas.Children.Remove(ripple);
            sb.Begin();
        }
    }
}
