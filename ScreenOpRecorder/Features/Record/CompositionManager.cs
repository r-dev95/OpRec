using Microsoft.Graphics.Canvas;

using ScreenOpRecorder.Features.Input;

using Windows.Graphics.Capture;

namespace ScreenOpRecorder.Features.Record
{
    public class CompositionManager
    {
        private readonly MouseHookService _mouseHookService;
        private readonly KeyboardHookService _keyboardHookService;
        private readonly GraphicsCaptureItem _item;

        private readonly FrameZoom _frameZoom;
        private readonly FrameOverlay _frameOverlay;

        public CompositionManager(MouseHookService mouseHookService, KeyboardHookService keyboardHookService, GraphicsCaptureItem item)
        {
            _mouseHookService = mouseHookService;
            _keyboardHookService = keyboardHookService;
            _item = item;

            _frameZoom = new FrameZoom(item.Size.Width, item.Size.Height);
            _frameOverlay = new FrameOverlay();

            _frameZoom.ZoomAction += _frameOverlay.OnZoomAction;

            _mouseHookService.MouseClicked += (x, y, isDouble) => {
                if (isDouble)
                {
                    _frameZoom.ToggleZoom(x, y);
                }
                _frameOverlay.AddRipple(x, y);
            };

            _keyboardHookService.KeyDown += (keyName) => {
                _frameOverlay.UpdateKey(keyName);
            };
        }

        public void ComposeFrame(CanvasRenderTarget renderTarget, CanvasBitmap rawFrame)
        {
            using (var ds = renderTarget.CreateDrawingSession())
            {
                // 画面フレーム描画（ズーム処理あり）
                _frameZoom.DrawZoomFrame(ds, renderTarget.Size, rawFrame);

                // オーバーレイ描画（キー表示）
                _frameOverlay.DrawKey(ds, renderTarget.Size);

                // オーバレイ描画（クリックリップル表示）
                _frameOverlay.DrawRipple(ds, renderTarget.Size);
            }
        }
    }
}
