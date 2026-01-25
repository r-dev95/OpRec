using Microsoft.Graphics.Canvas;

using Windows.Foundation;

namespace ScreenOpRecorder.Features.Record
{
    public class CompositionController
    {
        private CanvasDevice _device;
        private ZoomController _zoomController;

        public CompositionController(CanvasDevice device, int width, int height)
        {
            _device = device;
            _zoomController = new ZoomController(width, height);
        }

        public void ComposeFrame(CanvasDrawingSession ds, CanvasBitmap rawFrame, int mouseX, int mouseY)
        {
            // 1. ズーム範囲の計算
            Rect sourceRect = _zoomController.UpdateViewport(mouseX, mouseY);

            // 2. 背景（デスクトップ）のズーム描画
            // rawFrameのsourceRect部分を、出力画面全体（TargetRect）に拡大して描画
            Rect targetRect = new Rect(0, 0, _zoomController.ScreenSize.Width, _zoomController.ScreenSize.Height);
            ds.DrawImage(rawFrame, targetRect, sourceRect, 128.0f, CanvasImageInterpolation.Linear);

            // 3. ここに後ほど「キー表示」や「マウス波紋」を「上書き」する
            // ズームの影響を受けないため、常に固定サイズで描画される
        }
    }
}
