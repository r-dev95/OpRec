using System;
using System.Numerics;
using System.Threading;

using Microsoft.Graphics.Canvas;

using ScreenOpRecorder.Features.Input;

using Windows.Foundation;
using Windows.Graphics.Capture;

namespace ScreenOpRecorder.Features.Record
{
    public class FrameZoom
    {
        private Vector2 _targetPos; // ズーム中心位置
        private Vector2 _originalCameraPos; // 全画面のカメラ位置
        private Vector2 _currentCameraPos; // 現在のカメラ位置
        private float _interpolationSpeed = 0.01f; // 追従の滑らかさ (0.0～1.0)

        private float _currentZoom = 1.0f; // 現在のズーム倍率
        private float _targetZoom = 1.0f;  // 目標のズーム倍率
        private const float ZoomMax = 2.0f;
        private const float ZoomMin = 1.0f;

        public Action<Rect>? ZoomAction;

        public FrameZoom(int width, int height)
        {
            _originalCameraPos = new Vector2((float)(width * 0.5), (float)(height * 0.5));
            _currentCameraPos = _originalCameraPos;
        }

        public void ToggleZoom(int mouseX, int mouseY)
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

        public Rect UpdateViewport(Size screenSize)
        {
            // ズーム倍率を滑らかに補間
            _currentZoom = _currentZoom + (_targetZoom - _currentZoom) * _interpolationSpeed;

            _currentCameraPos = Vector2.Lerp(_currentCameraPos, _targetPos, _interpolationSpeed);

            float viewWidth = (float)(screenSize.Width / _currentZoom);
            float viewHeight = (float)(screenSize.Height / _currentZoom);

            float left = _currentCameraPos.X - (viewWidth / 2);
            float top = _currentCameraPos.Y - (viewHeight / 2);

            left = Math.Clamp(left, 0, (float)screenSize.Width - viewWidth);
            top = Math.Clamp(top, 0, (float)screenSize.Height - viewHeight);

            return new Rect(left, top, viewWidth, viewHeight);
        }

        public void DrawZoomFrame(CanvasDrawingSession ds, Size screenSize, CanvasBitmap rawFrame)
        {
            Rect sourceRect = UpdateViewport(screenSize);

            Rect targetRect = new Rect(0, 0, screenSize.Width, screenSize.Height);

            ds.DrawImage(rawFrame, targetRect, sourceRect, 1.0f, CanvasImageInterpolation.Linear);

            ZoomAction?.Invoke(sourceRect);
        }
    }
}
