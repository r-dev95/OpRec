using System;

using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;

using ScreenOpRecorder.Shared.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ScreenOpRecorder.Features.Overlay
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OverlayWindow : Window
    {
        private readonly ILogger _logger;
        private readonly OverlayViewModel ViewModel;

        public OverlayWindow(ILogger<OverlayWindow> logger, OverlayViewModel viewModel)
        {
            InitializeComponent();
            _logger = logger;
            ViewModel = viewModel;

            SetWindow();

            ViewModel.SetRecordingWindow += OnSetRecordingWindow;
            ViewModel.SetNotRecordingWindow += OnSetNotRecordingWindow;
            ViewModel.RippleRequested += OnRippleRequested;
            ViewModel.SetScaleFactor(WindowHelper.GetScaleFactor(this));
            ViewModel.Start();
        }

        public void Stop()
        {
            ViewModel?.SetRecordingWindow -= OnSetRecordingWindow;
            ViewModel?.SetNotRecordingWindow -= OnSetNotRecordingWindow;
            ViewModel?.RippleRequested -= OnRippleRequested;
            ViewModel?.Stop();
        }

        private void OnSetRecordingWindow()
        {
            WindowHelper.SetAlwaysOnTop(this, true);
            WindowHelper.SetClickThrough(this, true);
            MaskPath.Visibility = Visibility.Collapsed;
            ViewModel.CanSubmit = false;
            var offset = CaptureArea.StrokeThickness;
            ViewModel.Selection.CaptureAreaRect = new(
                ViewModel.Selection.X - offset,
                ViewModel.Selection.Y - offset,
                ViewModel.Selection.Width + 2 * offset,
                ViewModel.Selection.Height + 2 * offset);
            ViewModel.InputFeedback.SetRecordingState(true, ViewModel.Selection.CaptureAreaRect);
        }

        private void OnSetNotRecordingWindow()
        {
            WindowHelper.SetAlwaysOnTop(this, false);
            WindowHelper.SetClickThrough(this, false);
            MaskPath.Visibility = Visibility.Visible;
            ViewModel.Selection.IsCaptureAreaVisible = Visibility.Collapsed;
            ViewModel.CanSubmit = true;
            ViewModel.Selection.CaptureAreaRect = new(0, 0, 0, 0);
            ViewModel.InputFeedback.SetRecordingState(false, ViewModel.Selection.CaptureAreaRect);
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
                StrokeThickness = 2,
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

        private void SetWindow()
        {
            WindowHelper.SetBorderAndTitleBar(this, false, false);
            WindowHelper.MaximizeWindow(this);
            var scale = WindowHelper.GetScaleFactor(this);
            var physicalBounds = DpiHelper.ToPhysical(new Windows.Foundation.Size(Bounds.Width, Bounds.Height), scale);
            FullAreaRect.Rect = new(0, 0, physicalBounds.Width, physicalBounds.Height);
            ViewModel.InputFeedback.SetScreenSize(Bounds.Width, Bounds.Height);
            ViewModel.InputFeedback.SetRecordingState(false, ViewModel.Selection.CaptureAreaRect);
        }
    }
}

