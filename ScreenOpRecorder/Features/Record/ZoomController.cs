using System;

using System.Numerics;

using Windows.Foundation;

namespace ScreenOpRecorder.Features.Record
{
    public class ZoomController
    {
        private Vector2 _currentCameraPos; // 現在のカメラ位置
        private float _interpolationSpeed = 0.15f; // 追従の滑らかさ (0.0～1.0)
        private float _zoomFactor = 1.0f; // 2倍ズーム

        public Size ScreenSize{ get; set; }

        public ZoomController(int width, int height)
        {
            ScreenSize = new Size(width, height);
            _currentCameraPos = new Vector2((float)(width * 0.5), (float)(height * 0.5));
        }

        // 毎フレーム呼び出し、新しい表示領域（Rect）を計算する
        public Rect UpdateViewport(int mouseX, int mouseY)
        {
            // マウスの目標座標
            Vector2 targetPos = new Vector2(mouseX, mouseY);

            // 線形補間 (Lerp) でカメラを目標に近づける
            _currentCameraPos = Vector2.Lerp(_currentCameraPos, targetPos, _interpolationSpeed);

            // ズーム後のサイズを計算
            float viewWidth = (float)(ScreenSize.Width / _zoomFactor);
            float viewHeight = (float)(ScreenSize.Height / _zoomFactor);

            // カメラ位置を中心に据えた矩形を作成
            float left = _currentCameraPos.X - (viewWidth / 2);
            float top = _currentCameraPos.Y - (viewHeight / 2);

            // 画面外にはみ出さないように補正 (Clamping)
            left = Math.Clamp(left, 0, (float)ScreenSize.Width - viewWidth);
            top = Math.Clamp(top, 0, (float)ScreenSize.Height - viewHeight);

            return new Rect(left, top, viewWidth, viewHeight);
        }
    }
}
