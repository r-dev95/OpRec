using System;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

using ScreenOpRecorder.Features.Input;
using ScreenOpRecorder.Shared.Helpers;
using ScreenOpRecorder.Shared.Messages;

using Windows.Foundation;

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
                Minimap.Reset();
            });

            _messenger.Register<ZoomAreaChangedMessage>(this, (r, m) =>
            {
                if (_isRecording)
                {
                    _dispatcherQueue?.TryEnqueue(() =>
                    {
                        // ZoomRectは物理ピクセルなので、論理ピクセルに変換する
                        KeyDisplayArea = DpiHelper.ToLogical(m.ZoomRect, _scaleFactor);

                        // ズーム判定とミニマップ更新を MinimapViewModel に委譲
                        Minimap.Update(CaptureAreaRect, KeyDisplayArea);
                    });
                }
            });
        }

        [ObservableProperty]
        public partial MinimapViewModel Minimap { get; set; } = new();

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

        public Rect GetCaptureRect() => DpiHelper.ToPhysical(new Rect(X, Y, Width, Height), _scaleFactor);

        private void OnMouseClicked(int x, int y, bool isDouble)
        {
            // DPIを考慮した座標変換 (物理ピクセル -> 論理ピクセル)
            var logicalPoint = DpiHelper.ToLogical(new Point(x, y), _scaleFactor);

            RippleRequested?.Invoke(logicalPoint.X, logicalPoint.Y, isDouble);
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
