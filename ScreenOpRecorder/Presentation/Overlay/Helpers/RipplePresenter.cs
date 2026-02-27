using System;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;

using ScreenOpRecorder.Core.Settings.Models;

using Windows.UI;

namespace ScreenOpRecorder.Presentation.Overlay.Helpers
{
    internal sealed class RipplePresenter
    {
        private static readonly Color DoubleClickColor = Color.FromArgb(255, 255, 69, 0);
        private static readonly Color FallbackColor = Color.FromArgb(255, 0, 255, 255);
        private const int DurationMs = 500;

        private readonly Canvas _overlayCanvas;

        public RipplePresenter(Canvas overlayCanvas)
        {
            _overlayCanvas = overlayCanvas;
        }

        public void Show(double x, double y, ClickHighlightSettings settings, bool isDouble)
        {
            if (!settings.Enabled)
            {
                return;
            }

            var size = settings.Size;
            var stroke = isDouble
                ? DoubleClickColor
                : HexColorParser.ParseOrDefault(settings.ColorHex, FallbackColor);
            var strokeThickness = Math.Max(1.0, size / 10.0);

            var ripple = new Ellipse
            {
                Width = size,
                Height = size,
                Stroke = new SolidColorBrush(stroke),
                StrokeThickness = strokeThickness,
                RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5),
                RenderTransform = new ScaleTransform { ScaleX = 1, ScaleY = 1 }
            };

            Canvas.SetLeft(ripple, x - 0.5 * size);
            Canvas.SetTop(ripple, y - 0.5 * size);
            _overlayCanvas.Children.Add(ripple);

            var storyboard = CreateStoryboard(ripple);
            storyboard.Completed += (_, _) => _overlayCanvas.Children.Remove(ripple);
            storyboard.Begin();
        }

        private static Storyboard CreateStoryboard(Ellipse ripple)
        {
            var storyboard = new Storyboard();

            var scaleXAnimation = new DoubleAnimation { To = 3, Duration = TimeSpan.FromMilliseconds(DurationMs) };
            Storyboard.SetTarget(scaleXAnimation, ripple.RenderTransform);
            Storyboard.SetTargetProperty(scaleXAnimation, "ScaleX");

            var scaleYAnimation = new DoubleAnimation { To = 3, Duration = TimeSpan.FromMilliseconds(DurationMs) };
            Storyboard.SetTarget(scaleYAnimation, ripple.RenderTransform);
            Storyboard.SetTargetProperty(scaleYAnimation, "ScaleY");

            var opacityAnimation = new DoubleAnimation { To = 0, Duration = TimeSpan.FromMilliseconds(DurationMs) };
            Storyboard.SetTarget(opacityAnimation, ripple);
            Storyboard.SetTargetProperty(opacityAnimation, "Opacity");

            storyboard.Children.Add(scaleXAnimation);
            storyboard.Children.Add(scaleYAnimation);
            storyboard.Children.Add(opacityAnimation);

            return storyboard;
        }
    }
}

