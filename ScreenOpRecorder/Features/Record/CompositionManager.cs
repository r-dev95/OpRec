using System;

using Microsoft.Graphics.Canvas;

using ScreenOpRecorder.Features.Input;

using Windows.Foundation;

namespace ScreenOpRecorder.Features.Record
{
    public class CompositionManager
    {
        private readonly MouseHookService _mouseHookService;
        private readonly KeyboardHookService _keyboardHookService;
        private readonly Rect _captureArea;

        private readonly FrameZoom _frameZoom;
        private readonly FrameOverlay _frameOverlay;

        public event Action<Rect>? ZoomChanged;

        public CompositionManager(MouseHookService mouseHookService, KeyboardHookService keyboardHookService, Rect captureArea)
        {
            _mouseHookService = mouseHookService;
            _keyboardHookService = keyboardHookService;
            _captureArea = captureArea;

            _frameZoom = new FrameZoom(_captureArea);
            _frameOverlay = new FrameOverlay();

            _frameZoom.ZoomAction += (rect) =>
            {
                _frameOverlay.OnZoomAction(rect);
                ZoomChanged?.Invoke(rect);
            };

            _mouseHookService.MouseClicked += (x, y, isDouble) =>
            {
                float relativeX = (float)(x - _captureArea.X);
                float relativeY = (float)(y - _captureArea.Y);

                if (isDouble)
                {
                    _frameZoom.ToggleZoom(relativeX, relativeY);
                }
                _frameOverlay.AddRipple(relativeX, relativeY);
            };

            _keyboardHookService.KeyDown += (keyName) =>
            {
                _frameOverlay.UpdateKey(keyName);
            };
        }

        public void ComposeFrame(CanvasRenderTarget renderTarget, CanvasBitmap rawFrame)
        {
            using var ds = renderTarget.CreateDrawingSession();

            // 画面フレーム描画（ズーム処理あり）
            _frameZoom.DrawZoomFrame(ds, renderTarget.Size, rawFrame);

            // オーバーレイ描画（キー表示）
            //_frameOverlay.DrawKey(ds, renderTarget.Size);

            // オーバレイ描画（クリックリップル表示）
            //_frameOverlay.DrawRipple(ds, renderTarget.Size);
        }
    }
}
