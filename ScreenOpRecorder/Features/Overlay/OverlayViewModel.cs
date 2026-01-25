using System;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

using ScreenOpRecorder.Features.Input;

namespace ScreenOpRecorder.Features.Overlay
{
    public partial class OverlayViewModel : ObservableObject
    {
        private readonly ILogger<OverlayViewModel> _logger;

        private readonly MouseHookService _mouseHookService;
        private readonly KeyboardHookService _keyboardHookService;

        private CancellationTokenSource? _cts;

        private double _scaleFactor = 1.0;

        [ObservableProperty]
        private string _currentKeyText = "";

        public event Action<double, double, bool>? RippleRequested;

        public OverlayViewModel(ILogger<OverlayViewModel> logger, MouseHookService mouseHookService, KeyboardHookService keyboardHookService)
        {
            _logger = logger;
            _mouseHookService = mouseHookService;
            _mouseHookService.MouseClicked += OnMouseClicked;
            _keyboardHookService = keyboardHookService;
            _keyboardHookService.KeyDown += OnKeyDown;
        }

        public void Start()
        {
            _mouseHookService.Start();
            _keyboardHookService.Start();
        }

        public void SetScaleFactor(double scaleFactor)
        {
            _scaleFactor = scaleFactor;
            _logger.LogDebug("OverlayViewModel initialized with scale factor: {Scale}", _scaleFactor);
        }

        public Visibility IsVisibility(string text)
        {
            return string.IsNullOrEmpty(text) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void OnMouseClicked(int x, int y, bool isDouble)
        {
            // DPIを考慮した座標変換 (物理ピクセル -> 論理ピクセル)
            double logicalX = x / _scaleFactor;
            double logicalY = y / _scaleFactor;

            RippleRequested?.Invoke(logicalX, logicalY, isDouble);
        }

        private async void OnKeyDown(string keyName)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            CurrentKeyText += keyName + " ";

            try
            {
                await Task.Delay(1500, _cts.Token);
                CurrentKeyText = "";
            }
            catch { }
        }
    }
}
