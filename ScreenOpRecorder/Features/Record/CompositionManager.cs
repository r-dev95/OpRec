using System;

using Microsoft.Graphics.Canvas;

using ScreenOpRecorder.Features.Input;

using Windows.Foundation;

namespace ScreenOpRecorder.Features.Record
{
    public class CompositionManager : IDisposable
    {
        private readonly MouseHookService _mouseHookService;
        private readonly KeyboardHookService _keyboardHookService;
        private readonly Rect _captureArea;

        private readonly FrameZoom _frameZoom;
        private readonly FrameOverlay _frameOverlay;

        public event Action<Rect>? ZoomChanged;

        public CompositionManager(MouseHookService mouseHookService, KeyboardHookService keyboardHookService, Rect captureArea, double zoomFactor)
        {
            _mouseHookService = mouseHookService;
            _keyboardHookService = keyboardHookService;
            _captureArea = captureArea;

            _frameZoom = new FrameZoom(_captureArea, zoomFactor);
            _frameOverlay = new FrameOverlay();

            _frameZoom.ZoomAction += OnZoomAction;
            _mouseHookService.MouseClicked += OnMouseClicked;
            _keyboardHookService.KeyDown += OnKeyDown;
        }

        public void Dispose()
        {
            _frameZoom?.ZoomAction -= OnZoomAction;
            _mouseHookService?.MouseClicked -= OnMouseClicked;
            _keyboardHookService?.KeyDown -= OnKeyDown;
        }

        public void ComposeFrame(CanvasRenderTarget renderTarget, CanvasBitmap rawFrame)
        {
            using var ds = renderTarget.CreateDrawingSession();

            _frameZoom.DrawZoomFrame(ds, renderTarget.Size, rawFrame);

            //_frameOverlay.DrawKey(ds, renderTarget.Size);

            //_frameOverlay.DrawRipple(ds, renderTarget.Size);
        }

        private void OnZoomAction(Rect rect)
        {
            _frameOverlay.OnZoomAction(rect);
            ZoomChanged?.Invoke(rect);
        }

        private void OnMouseClicked(int x, int y, bool isDouble)
        {
            float relativeX = (float)(x - _captureArea.X);
            float relativeY = (float)(y - _captureArea.Y);

            if (isDouble)
            {
                _frameZoom.ToggleZoom(relativeX, relativeY);
            }
            _frameOverlay.AddRipple(relativeX, relativeY);
        }

        private void OnKeyDown(string keyName)
        {
            _frameOverlay.UpdateKey(keyName);
        }
    }
}
