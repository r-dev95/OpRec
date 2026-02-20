using System;
using System.Numerics;

using Microsoft.Graphics.Canvas;

using Windows.Foundation;

namespace ScreenOpRecorder.Features.Record
{
    public class FrameZoom
    {
        private const float InterpolationSpeed = 0.01f;

        private readonly Rect _captureArea;
        private readonly Vector2 _originalCameraPos;
        private readonly float _zoomMax;
        private Vector2 _currentCameraPos;
        private Vector2 _targetPos;

        private float _currentZoom = 1.0f;
        private float _targetZoom = 1.0f;
        private const float ZoomMin = 1.0f;

        public Action<Rect>? ZoomAction;

        public FrameZoom(Rect captureArea, double zoomMax = 2.0)
        {
            _captureArea = captureArea;
            _zoomMax = (float)Math.Clamp(zoomMax, 1.1, 4.0);

            _originalCameraPos = new Vector2((float)(captureArea.Width * 0.5), (float)(captureArea.Height * 0.5));
            _currentCameraPos = _originalCameraPos;
        }

        public void ToggleZoom(float mouseX, float mouseY)
        {
            if (_targetZoom == ZoomMin)
            {
                _targetZoom = _zoomMax;
                _currentCameraPos = _originalCameraPos;
                _targetPos = new Vector2(mouseX, mouseY);
            }
            else
            {
                _targetZoom = ZoomMin;
                _targetPos = new Vector2(_originalCameraPos.X, _originalCameraPos.Y);
            }
        }

        public Rect UpdateViewport(double width, double height)
        {
            _currentZoom += (_targetZoom - _currentZoom) * InterpolationSpeed;
            _currentCameraPos = Vector2.Lerp(_currentCameraPos, _targetPos, InterpolationSpeed);

            float viewWidth = (float)(width / _currentZoom);
            float viewHeight = (float)(height / _currentZoom);

            float left = _currentCameraPos.X - (viewWidth / 2);
            float top = _currentCameraPos.Y - (viewHeight / 2);

            left = Math.Clamp(left, 0, (float)width - viewWidth);
            top = Math.Clamp(top, 0, (float)height - viewHeight);

            return new Rect(left, top, viewWidth, viewHeight);
        }

        public void DrawZoomFrame(CanvasDrawingSession ds, Size targetSize, CanvasBitmap rawFrame)
        {
            Rect zoomRect = UpdateViewport(_captureArea.Width, _captureArea.Height);

            Rect sourceRect = new(
                _captureArea.X + zoomRect.X,
                _captureArea.Y + zoomRect.Y,
                zoomRect.Width,
                zoomRect.Height);

            Rect targetRect = new(0, 0, targetSize.Width, targetSize.Height);

            ds.DrawImage(rawFrame, targetRect, sourceRect, 1.0f, CanvasImageInterpolation.Linear);
            ZoomAction?.Invoke(sourceRect);
        }
    }
}
