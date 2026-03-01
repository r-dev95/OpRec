using System;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Common.Helpers;
using ScreenOpRecorder.Application.Input;
using ScreenOpRecorder.Application.Recording;
using ScreenOpRecorder.Application.Recording.State;
using ScreenOpRecorder.Application.Settings.Interfaces;
using ScreenOpRecorder.Application.Settings.Models;
using ScreenOpRecorder.Domain.ValueObjects;

using Windows.Foundation;

namespace ScreenOpRecorder.Presentation.Overlay
{
    public partial class OverlayViewModel : ObservableObject
    {
        private readonly ILogger<OverlayViewModel> _logger;
        private readonly IUserSettingsService _settingsService;
        private readonly IInputEventListener _inputEventListener;
        private readonly IRecordingSessionStore _stateStore;
        private readonly ISelectCaptureAreaUseCase _selectCaptureAreaUseCase;
        private readonly Microsoft.UI.Dispatching.DispatcherQueue? _dispatcherQueue;

        private double _scaleFactor = 1.0;
        private bool _isRecording;
        private bool _isSettingsSubscribed;

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

        public ClickHighlightSettings ClickHighlight = ClickHighlightSettings.Default;

        public event Action? SetRecordingWindow;
        public event Action? SetNotRecordingWindow;
        public event Action<double, double, bool>? RippleRequested;

        public OverlayViewModel(
            ILogger<OverlayViewModel> logger,
            IUserSettingsService settingsService,
            IInputEventListener inputEventListener,
            IRecordingSessionStore stateStore,
            ISelectCaptureAreaUseCase selectCaptureAreaUseCase)
        {
            _logger = logger;
            _stateStore = stateStore;
            _settingsService = settingsService;
            _inputEventListener = inputEventListener;
            _selectCaptureAreaUseCase = selectCaptureAreaUseCase;

            try
            {
                _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            }
            catch
            {
            }

            ApplySettings(_settingsService.Current);
        }

        public void Start()
        {
            _stateStore.StateChanged += OnRecordingStateChanged;
            if (!_isSettingsSubscribed)
            {
                _settingsService.SettingsChanged += OnSettingsChanged;
                _isSettingsSubscribed = true;
            }
            _inputEventListener.MouseClicked += OnMouseClicked;
            _inputEventListener.KeyDown += OnKeyDown;

            CanSubmit = true;
            Selection.ClearSelection();
        }

        public void Stop()
        {
            _stateStore.StateChanged -= OnRecordingStateChanged;
            if (_isSettingsSubscribed)
            {
                _settingsService.SettingsChanged -= OnSettingsChanged;
                _isSettingsSubscribed = false;
            }
            _inputEventListener.MouseClicked -= OnMouseClicked;
            _inputEventListener.KeyDown -= OnKeyDown;
        }

        public void SetScaleFactor(double scaleFactor)
        {
            _scaleFactor = scaleFactor;
            _logger.LogDebug("OverlayViewModel initialized with scale factor: {Scale}", _scaleFactor);
        }

        public void InitializeWindowState(double width, double height)
        {
            InputFeedback.SetScreenSize(width, height);
            InputFeedback.ApplySession(false, Selection.CaptureAreaRect, null);
        }

        public ScreenRect GetCaptureRect()
        {
            var physical = DpiHelper.ToPhysical(Selection.GetSelectionRect(), _scaleFactor);
            return new ScreenRect(physical.X, physical.Y, physical.Width, physical.Height);
        }

        public void EnterRecordingUiState(double captureStrokeThickness)
        {
            CanSubmit = false;
            Selection.EnterRecordingUiState(captureStrokeThickness);
            InputFeedback.ApplySession(true, Selection.CaptureAreaRect, null);
        }

        public void ExitRecordingUiState()
        {
            CanSubmit = true;
            Selection.ExitRecordingUiState();
            InputFeedback.ApplySession(false, Selection.CaptureAreaRect, null);
        }

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

            _selectCaptureAreaUseCase.SelectCaptureArea(GetCaptureRect());
        }

        private void OnRecordingStateChanged(RecordingSessionState state)
        {
            if (_dispatcherQueue != null)
            {
                _dispatcherQueue.TryEnqueue(() => ApplySessionState(state));
                return;
            }

            ApplySessionState(state);
        }

        private void OnSettingsChanged(UserSettings settings)
        {
            if (_dispatcherQueue != null)
            {
                _dispatcherQueue.TryEnqueue(() => ApplySettings(settings));
                return;
            }

            ApplySettings(settings);
        }

        private void ApplySettings(UserSettings settings)
        {
            ClickHighlight = new ClickHighlightSettings(
                settings.EnableClickHighlight,
                settings.ClickHighlightColor,
                settings.ClickHighlightSize);
            InputFeedback.ApplySettings(settings);
            Minimap.ApplySession(_isRecording, settings.EnableMinimap, Selection.CaptureAreaRect, InputFeedback.KeyDisplayArea);
        }

        private void ApplySessionState(RecordingSessionState state)
        {
            if (_isRecording != state.IsRecording)
            {
                _isRecording = state.IsRecording;
                if (_isRecording)
                {
                    SetRecordingWindow?.Invoke();
                }
                else
                {
                    SetNotRecordingWindow?.Invoke();
                }
            }

            Rect? logicalZoomRect = null;
            if (state.IsRecording)
            {
                logicalZoomRect = DpiHelper.ToLogical(
                    new Rect(state.ZoomArea.X, state.ZoomArea.Y, state.ZoomArea.Width, state.ZoomArea.Height),
                    _scaleFactor);
            }

            InputFeedback.ApplySession(state.IsRecording, Selection.CaptureAreaRect, logicalZoomRect);
            Minimap.ApplySession(
                state.IsRecording,
                _settingsService.Current.EnableMinimap,
                Selection.CaptureAreaRect,
                InputFeedback.KeyDisplayArea);
        }
    }
}


