using System;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

using ScreenOpRecorder.Features.Input;
using ScreenOpRecorder.Shared.Messages;

using Windows.Foundation;

using static ScreenOpRecorder.Features.Shell.ShellViewModel;

namespace ScreenOpRecorder.Features.Overlay
{
    public partial class OverlayViewModel : ObservableObject
    {
        private readonly ILogger<OverlayViewModel> _logger;
        private readonly IMessenger _messenger;
        private readonly MouseHookService _mouseHookService;
        private readonly KeyboardHookService _keyboardHookService;

        [ObservableProperty]
        public partial double X
        {
            get; set;
        }

        [ObservableProperty]
        public partial double Y
        {
            get; set;
        }

        [ObservableProperty]
        public partial double Width
        {
            get; set;
        }

        partial void OnWidthChanged(double value) => OnPropertyChanged(nameof(SizeLabel));

        [ObservableProperty]
        public partial double Height
        {
            get; set;
        }

        partial void OnHeightChanged(double value) => OnPropertyChanged(nameof(SizeLabel));

        [ObservableProperty]
        public partial Rect CaptureAreaRect
        {
            get; set;
        }

        [ObservableProperty]
        public partial Visibility IsCaptureAreaVisible
        {
            get; set;
        }

        [ObservableProperty]
        public partial Visibility IsSizeTagVisible
        {
            get; set;
        }

        [ObservableProperty]
        public partial string CurrentKeyText { get; set; } = "";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PointerPressedCommand))]
        [NotifyCanExecuteChangedFor(nameof(PointerMovedCommand))]
        [NotifyCanExecuteChangedFor(nameof(PointerReleasedCommand))]
        public partial bool CanSubmit
        {
            get; set;
        }

        public string SizeLabel => $"{(int)Width} x {(int)Height}";

        private Point _startPoint;
        private bool _isSelecting = false;
        public record SelectionCompletedMessage(Rect captureRect);

        public event Action? SetRecordingWindow;
        public event Action? SetNotRecordingWindow;
        public event Action<double, double, bool>? RippleRequested;

        private CancellationTokenSource? _cts;

        private double _scaleFactor = 1.0;

        [ObservableProperty]
        public partial Rect KeyDisplayArea
        {
            get; set;
        }

        private Size _screenSize;
        private bool _isRecording;
        private Microsoft.UI.Dispatching.DispatcherQueue? _dispatcherQueue;

        public OverlayViewModel(ILogger<OverlayViewModel> logger, IMessenger messenger, MouseHookService mouseHookService, KeyboardHookService keyboardHookService)
        {
            try
            {
                _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            }
            catch { }

            _logger = logger;
            _messenger = messenger;
            _mouseHookService = mouseHookService;
            _mouseHookService.MouseClicked += OnMouseClicked;
            _keyboardHookService = keyboardHookService;
            _keyboardHookService.KeyDown += OnKeyDown;

            _messenger.Register<StartRecordMessage>(this, (r, m) =>
            {
                SetRecordingWindow?.Invoke();
                _isRecording = true;
                UpdateKeyDisplayArea();
            });

            _messenger.Register<StopRecordMessage>(this, (r, m) =>
            {
                SetNotRecordingWindow?.Invoke();
                _isRecording = false;
                UpdateKeyDisplayArea();
                IsZoomedVisible = Visibility.Collapsed;
            });

            _messenger.Register<ZoomAreaChangedMessage>(this, (r, m) =>
            {
                if (_isRecording)
                {
                    _dispatcherQueue?.TryEnqueue(() =>
                    {
                        // ZoomRectは物理ピクセルなので、論理ピクセルに変換する
                        double x = m.ZoomRect.X / _scaleFactor;
                        double y = m.ZoomRect.Y / _scaleFactor;
                        double w = m.ZoomRect.Width / _scaleFactor;
                        double h = m.ZoomRect.Height / _scaleFactor;

                        KeyDisplayArea = new Rect(x, y, w, h);

                        // ズーム判定
                        bool isZoomed = w < (CaptureAreaRect.Width - 1.0);
                        IsZoomedVisible = isZoomed ? Visibility.Visible : Visibility.Collapsed;

                        if (isZoomed)
                        {
                            UpdateMinimap(KeyDisplayArea);
                        }
                    });
                }
            });
        }

        [ObservableProperty]
        public partial Visibility IsZoomedVisible { get; set; } = Visibility.Collapsed;

        [ObservableProperty]
        public partial double MinimapWidth { get; set; } = 150;

        [ObservableProperty]
        public partial double MinimapHeight
        {
            get; set;
        }

        [ObservableProperty]
        public partial double MinimapViewportX
        {
            get; set;
        }

        [ObservableProperty]
        public partial double MinimapViewportY
        {
            get; set;
        }

        [ObservableProperty]
        public partial double MinimapViewportWidth
        {
            get; set;
        }

        [ObservableProperty]
        public partial double MinimapViewportHeight
        {
            get; set;
        }

        private void UpdateMinimap(Rect currentViewport)
        {
            if (CaptureAreaRect.Width == 0 || CaptureAreaRect.Height == 0)
                return;

            double scale = MinimapWidth / CaptureAreaRect.Width;
            MinimapHeight = CaptureAreaRect.Height * scale;

            MinimapViewportX = (currentViewport.X - CaptureAreaRect.X) * scale;
            MinimapViewportY = (currentViewport.Y - CaptureAreaRect.Y) * scale;
            MinimapViewportWidth = currentViewport.Width * scale;
            MinimapViewportHeight = currentViewport.Height * scale;
        }

        public void Start()
        {
            _mouseHookService.Start();
            _keyboardHookService.Start();

            CanSubmit = true;
            IsCaptureAreaVisible = Visibility.Collapsed;
            IsSizeTagVisible = Visibility.Collapsed;
        }

        public void SetScreenSize(double width, double height)
        {
            _screenSize = new Size(width, height);
            UpdateKeyDisplayArea();
        }

        public void SetScaleFactor(double scaleFactor)
        {
            _scaleFactor = scaleFactor;
            _logger.LogDebug("OverlayViewModel initialized with scale factor: {Scale}", _scaleFactor);
        }

        public void SetCaptureRect(Point startPoint, Point currentPoint)
        {
            X = Math.Min(startPoint.X, currentPoint.X);
            Y = Math.Min(startPoint.Y, currentPoint.Y);
            Width = Math.Abs(startPoint.X - currentPoint.X);
            Height = Math.Abs(startPoint.Y - currentPoint.Y);
        }

        private void UpdateKeyDisplayArea()
        {
            if (_isRecording)
            {
                KeyDisplayArea = CaptureAreaRect;
            }
            else
            {
                KeyDisplayArea = new Rect(0, 0, _screenSize.Width, _screenSize.Height);
            }
        }

        public Rect GetCaptureRect() => new(X * _scaleFactor, Y * _scaleFactor, Width * _scaleFactor, Height * _scaleFactor);

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

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private void OnPointerPressed(Point position)
        {
            _isSelecting = true;
            _startPoint = position;
            IsCaptureAreaVisible = Visibility.Visible;
            IsSizeTagVisible = Visibility.Visible;
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private void OnPointerMoved(Point currentPoint)
        {
            if (!_isSelecting)
            {
                return;
            }
            SetCaptureRect(_startPoint, currentPoint);
            CaptureAreaRect = new(X, Y, Width, Height);
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private void OnPointerReleased()
        {
            _isSelecting = false;
            IsSizeTagVisible = Visibility.Collapsed;
            _messenger.Send(new SelectionCompletedMessage(GetCaptureRect()));
        }
    }
}
