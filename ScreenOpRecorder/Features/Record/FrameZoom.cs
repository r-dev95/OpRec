using System;
using System.Numerics;

using Microsoft.Graphics.Canvas;

using Windows.Foundation;

namespace ScreenOpRecorder.Features.Record
{
    public class FrameZoom
    {
        private readonly Rect _captureArea;
        private readonly Vector2 _originalCameraPos; // 全画面のカメラ位置
        private Vector2 _currentCameraPos; // 現在のカメラ位置
        private Vector2 _targetPos; // ズーム中心位置
        private float _interpolationSpeed = 0.01f; // 追従の滑らかさ (0.0～1.0)

        private float _currentZoom = 1.0f; // 現在のズーム倍率
        private float _targetZoom = 1.0f;  // 目標のズーム倍率
        private const float ZoomMax = 2.0f;
        private const float ZoomMin = 1.0f;

        public Action<Rect>? ZoomAction;

        public FrameZoom(Rect captureArea)
        {
            _captureArea = captureArea;

            _originalCameraPos = new Vector2((float)(captureArea.Width * 0.5), (float)(captureArea.Height * 0.5));
            _currentCameraPos = _originalCameraPos;
        }

        public void ToggleZoom(float mouseX, float mouseY)
        {
            if (_targetZoom == ZoomMin) // ズームイン
            {
                _targetZoom = ZoomMax;
                _currentCameraPos = _originalCameraPos;
                _targetPos = new Vector2(mouseX, mouseY);
            }
            else // ズームアウト
            {
                _targetZoom = ZoomMin;
                _targetPos = new Vector2(_originalCameraPos.X, _originalCameraPos.Y);
            }
        }

        public Rect UpdateViewport(double width, double height)
        {
            // ズーム倍率を滑らかに補間
            _currentZoom = _currentZoom + (_targetZoom - _currentZoom) * _interpolationSpeed;

            _currentCameraPos = Vector2.Lerp(_currentCameraPos, _targetPos, _interpolationSpeed);

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
                zoomRect.Height
            );

            Rect targetRect = new Rect(0, 0, targetSize.Width, targetSize.Height);

            ds.DrawImage(rawFrame, targetRect, sourceRect, 1.0f, CanvasImageInterpolation.Linear);

            ZoomAction?.Invoke(sourceRect);
        }
    }
}
