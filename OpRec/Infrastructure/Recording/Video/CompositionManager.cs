using System;

using Microsoft.Graphics.Canvas;

using OpRec.Application.Input.Ports;

using Windows.Foundation;

namespace OpRec.Infrastructure.Recording.Video
{
    public class CompositionManager : IDisposable
    {
        private readonly IMouseInputListener _mouseInputListener;
        private readonly Rect _captureArea;
        private readonly FrameZoom _frameZoom;

        public event Action<Rect>? ZoomChanged;

        public CompositionManager(IMouseInputListener mouseInputListener, Rect captureArea, double zoomFactor)
        {
            _mouseInputListener = mouseInputListener;
            _captureArea = captureArea;
            _frameZoom = new FrameZoom(_captureArea, zoomFactor);

            _frameZoom.ZoomAction += OnZoomAction;
            _mouseInputListener.MouseClicked += OnMouseClicked;
        }

        public void Dispose()
        {
            _frameZoom.ZoomAction -= OnZoomAction;
            _mouseInputListener.MouseClicked -= OnMouseClicked;
        }

        public void ComposeFrame(CanvasRenderTarget renderTarget, CanvasBitmap rawFrame)
        {
            using var drawingSession = renderTarget.CreateDrawingSession();
            _frameZoom.DrawZoomFrame(drawingSession, renderTarget.Size, rawFrame);
        }

        public void ToggleZoomAt(float relativeX, float relativeY)
        {
            _frameZoom.ToggleZoom(relativeX, relativeY);
        }

        private void OnZoomAction(Rect rect)
        {
            ZoomChanged?.Invoke(rect);
        }

        private void OnMouseClicked(int x, int y, bool isDouble)
        {
            if (!isDouble)
            {
                return;
            }

            var relativeX = (float)(x - _captureArea.X);
            var relativeY = (float)(y - _captureArea.Y);
            _frameZoom.ToggleZoom(relativeX, relativeY);
        }
    }
}

