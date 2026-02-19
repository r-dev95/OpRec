using System;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.Logging;

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
        private readonly Microsoft.UI.Dispatching.DispatcherQueue? _dispatcherQueue;

        private double _scaleFactor = 1.0;
        private bool _isRecording;

        public event Action? SetRecordingWindow;
        public event Action? SetNotRecordingWindow;
        public event Action<double, double, bool>? RippleRequested;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PointerPressedCommand))]
        [NotifyCanExecuteChangedFor(nameof(PointerMovedCommand))]
        [NotifyCanExecuteChangedFor(nameof(PointerReleasedCommand))]
        public partial bool CanSubmit { get; set; }

        [ObservableProperty]
        public partial SelectionRegionModel Selection { get; set; } = new();

        [ObservableProperty]
        public partial InputFeedbackModel InputFeedback { get; set; } = new();

        [ObservableProperty]
        public partial MinimapModel Minimap { get; set; } = new();

        public OverlayViewModel(ILogger<OverlayViewModel> logger, IMessenger messenger, MouseHookService mouseHookService, KeyboardHookService keyboardHookService)
        {
            _logger = logger;
            _messenger = messenger;
            _mouseHookService = mouseHookService;
            _keyboardHookService = keyboardHookService;

            try
            {
                _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            }
            catch
            {
            }

            _messenger.Register<StartRecordMessage>(this, (r, m) =>
            {
                SetRecordingWindow?.Invoke();
                _isRecording = true;
                InputFeedback.SetRecordingState(true, Selection.CaptureAreaRect);
            });

            _messenger.Register<StopRecordMessage>(this, (r, m) =>
            {
                SetNotRecordingWindow?.Invoke();
                _isRecording = false;
                InputFeedback.SetRecordingState(false, Selection.CaptureAreaRect);
                Minimap.Reset();
            });

            _messenger.Register<ZoomAreaChangedMessage>(this, (r, m) =>
            {
                if (!_isRecording)
                {
                    return;
                }

                _dispatcherQueue?.TryEnqueue(() =>
                {
                    var logicalRect = DpiHelper.ToLogical(m.ZoomRect, _scaleFactor);
                    InputFeedback.SetZoomArea(logicalRect);
                    Minimap.Update(Selection.CaptureAreaRect, InputFeedback.KeyDisplayArea);
                });
            });
        }

        public void Start()
        {
            _mouseHookService.MouseClicked += OnMouseClicked;
            _keyboardHookService.KeyDown += OnKeyDown;

            _mouseHookService.Start();
            _keyboardHookService.Start();

            CanSubmit = true;
            Selection.ClearSelection();
        }

        public void Stop()
        {
            _mouseHookService.MouseClicked -= OnMouseClicked;
            _keyboardHookService.KeyDown -= OnKeyDown;

            _mouseHookService.Dispose();
            _keyboardHookService.Dispose();
        }

        public void SetScreenSize(double width, double height)
        {
            InputFeedback.SetScreenSize(width, height);
            InputFeedback.SetRecordingState(_isRecording, Selection.CaptureAreaRect);
        }

        public void SetScaleFactor(double scaleFactor)
        {
            _scaleFactor = scaleFactor;
            _logger.LogDebug("OverlayViewModel initialized with scale factor: {Scale}", _scaleFactor);
        }

        public Rect GetCaptureRect() => DpiHelper.ToPhysical(Selection.GetSelectionRect(), _scaleFactor);

        private void OnMouseClicked(int x, int y, bool isDouble)
        {
            var logicalPoint = DpiHelper.ToLogical(new Point(x, y), _scaleFactor);
            RippleRequested?.Invoke(logicalPoint.X, logicalPoint.Y, isDouble);
        }

        private async void OnKeyDown(string keyName)
        {
            await InputFeedback.ShowKeyAsync(keyName);
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private void OnPointerPressed(Point position)
        {
            Selection.BeginSelection(position);
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private void OnPointerMoved(Point currentPoint)
        {
            Selection.UpdateSelection(currentPoint);
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private void OnPointerReleased()
        {
            Selection.EndSelection();
            if (Selection.HasSelection)
            {
                _messenger.Send(new SelectionCompletedMessage(GetCaptureRect()));
            }
        }
    }
}
