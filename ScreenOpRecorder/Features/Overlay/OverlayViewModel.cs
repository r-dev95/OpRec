using System;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Features.Input;

namespace ScreenOpRecorder.Features.Overlay
{
    public partial class OverlayViewModel : ObservableObject
    {
        private readonly ILogger<OverlayViewModel> _logger;
        private readonly IMouseHookService _mouseHookService;
        private double _scaleFactor = 1.0;

        public event Action<double, double, bool>? RippleRequested;
        public event Action<string>? KeyStrokeRequested;

        public OverlayViewModel(ILogger<OverlayViewModel> logger, IMouseHookService mouseHookService)
        {
            _logger = logger;
            _mouseHookService = mouseHookService;
            _mouseHookService.MouseClicked += OnMouseClicked;
        }

        public void Initialize(double scaleFactor)
        {
            _scaleFactor = scaleFactor;
            _mouseHookService.Start();
            _logger.LogDebug("OverlayViewModel initialized with scale factor: {Scale}", _scaleFactor);
        }

        private void OnMouseClicked(int x, int y, bool isDouble)
        {
            // DPIを考慮した座標変換 (物理ピクセル -> 論理ピクセル)
            double logicalX = x / _scaleFactor;
            double logicalY = y / _scaleFactor;

            RippleRequested?.Invoke(logicalX, logicalY, isDouble);
        }
    }
}
