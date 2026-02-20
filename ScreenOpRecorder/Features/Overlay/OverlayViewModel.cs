using System;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Features.Input;
using ScreenOpRecorder.Features.Record;
using ScreenOpRecorder.Features.Record.State;
using ScreenOpRecorder.Shared.Helpers;

using Windows.Foundation;

namespace ScreenOpRecorder.Features.Overlay
{
    public partial class OverlayViewModel : ObservableObject
    {
        private readonly ILogger<OverlayViewModel> _logger;
        private readonly IRecordingDomainService _recordingDomainService;
        private readonly IRecordingStateStore _stateStore;
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
        public partial SelectionRegionState Selection { get; set; } = new();

        [ObservableProperty]
        public partial InputFeedbackState InputFeedback { get; set; } = new();

        [ObservableProperty]
        public partial MinimapState Minimap { get; set; } = new();

        public OverlayViewModel(
            ILogger<OverlayViewModel> logger,
            IRecordingDomainService recordingDomainService,
            IRecordingStateStore stateStore,
            MouseHookService mouseHookService,
            KeyboardHookService keyboardHookService)
        {
            _logger = logger;
            _recordingDomainService = recordingDomainService;
            _stateStore = stateStore;
            _mouseHookService = mouseHookService;
            _keyboardHookService = keyboardHookService;

            try
            {
                _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            }
            catch
            {
            }
        }

        public void Start()
        {
            _stateStore.StateChanged += OnRecordingStateChanged;
            _mouseHookService.MouseClicked += OnMouseClicked;
            _keyboardHookService.KeyDown += OnKeyDown;

            _mouseHookService.Start();
            _keyboardHookService.Start();

            CanSubmit = true;
            Selection.ClearSelection();
        }

        public void Stop()
        {
            _stateStore.StateChanged -= OnRecordingStateChanged;
            _mouseHookService.MouseClicked -= OnMouseClicked;
            _keyboardHookService.KeyDown -= OnKeyDown;

            _mouseHookService.Dispose();
            _keyboardHookService.Dispose();
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
            if (!Selection.HasSelection)
            {
                return;
            }

            _recordingDomainService.SelectCaptureArea(GetCaptureRect());
        }

        private void OnRecordingStateChanged(RecordingState state)
        {
            _dispatcherQueue?.TryEnqueue(() =>
            {
                if (_isRecording != state.IsRecording)
                {
                    _isRecording = state.IsRecording;
                    if (_isRecording)
                    {
                        SetRecordingWindow?.Invoke();
                        InputFeedback.SetRecordingState(true, Selection.CaptureAreaRect);
                    }
                    else
                    {
                        SetNotRecordingWindow?.Invoke();
                        InputFeedback.SetRecordingState(false, Selection.CaptureAreaRect);
                        Minimap.Reset();
                    }
                }

                if (!_isRecording)
                {
                    return;
                }

                var logicalRect = DpiHelper.ToLogical(state.ZoomArea, _scaleFactor);
                InputFeedback.SetZoomArea(logicalRect);
                Minimap.Update(Selection.CaptureAreaRect, InputFeedback.KeyDisplayArea);
            });
        }
    }
}
