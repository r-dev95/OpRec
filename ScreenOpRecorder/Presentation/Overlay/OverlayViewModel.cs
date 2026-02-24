using System;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

using ScreenOpRecorder.Core.Recording.Ports;
using ScreenOpRecorder.Core.Settings.Ports;
using ScreenOpRecorder.Core.Settings.Models;
using ScreenOpRecorder.Core.Recording.State;
using ScreenOpRecorder.Core.Recording.UseCases;
using ScreenOpRecorder.Common.Helpers;
using ScreenOpRecorder.Domain.ValueObjects;

using Windows.Foundation;

namespace ScreenOpRecorder.Presentation.Overlay
{
    public partial class OverlayViewModel : ObservableObject
    {
        private readonly ILogger<OverlayViewModel> _logger;
        private readonly IUserSettingsService _settingsService;
        private readonly IRecordingSessionStore _stateStore;
        private readonly IRecordingCommandUseCase _recordingCommandUseCase;
        private readonly IGlobalMouseHook _mouseHookService;
        private readonly IGlobalKeyboardHook _keyboardHookService;
        private readonly Microsoft.UI.Dispatching.DispatcherQueue? _dispatcherQueue;

        private double _scaleFactor = 1.0;
        private bool _isRecording;
        private bool _isSettingsSubscribed;

        public bool EnableClickHighlight { get; private set; } = true;
        public string ClickHighlightColor { get; private set; } = UserSettingsConstraints.DefaultClickHighlightColor;
        public double ClickHighlightSize { get; private set; } = UserSettingsConstraints.DefaultClickHighlightSize;

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

        public OverlayViewModel(ILogger<OverlayViewModel> logger, IUserSettingsService settingsService, IRecordingSessionStore stateStore, IRecordingCommandUseCase recordingCommandUseCase, IGlobalMouseHook mouseHookService, IGlobalKeyboardHook keyboardHookService)
        {
            _logger = logger;
            _recordingCommandUseCase = recordingCommandUseCase;
            _stateStore = stateStore;
            _settingsService = settingsService;
            _mouseHookService = mouseHookService;
            _keyboardHookService = keyboardHookService;

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
            if (_isSettingsSubscribed)
            {
                _settingsService.SettingsChanged -= OnSettingsChanged;
                _isSettingsSubscribed = false;
            }
            _mouseHookService.MouseClicked -= OnMouseClicked;
            _keyboardHookService.KeyDown -= OnKeyDown;

            _mouseHookService.Stop();
            _keyboardHookService.Stop();
        }

        public void SetScaleFactor(double scaleFactor)
        {
            _scaleFactor = scaleFactor;
            _logger.LogDebug("OverlayViewModel initialized with scale factor: {Scale}", _scaleFactor);
        }

        public ScreenRect GetCaptureRect()
        {
            var physical = DpiHelper.ToPhysical(Selection.GetSelectionRect(), _scaleFactor);
            return new ScreenRect(physical.X, physical.Y, physical.Width, physical.Height);
        }

        public void EnterRecordingUiState(double captureStrokeThickness)
        {
            CanSubmit = false;
            var offset = captureStrokeThickness;
            Selection.CaptureAreaRect = new(
                Selection.X - offset,
                Selection.Y - offset,
                Selection.Width + 2 * offset,
                Selection.Height + 2 * offset);
            InputFeedback.SetRecordingState(true, Selection.CaptureAreaRect);
        }

        public void ExitRecordingUiState()
        {
            Selection.IsCaptureAreaVisible = Visibility.Collapsed;
            CanSubmit = true;
            Selection.CaptureAreaRect = new(0, 0, 0, 0);
            InputFeedback.SetRecordingState(false, Selection.CaptureAreaRect);
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

            _recordingCommandUseCase.SelectCaptureArea(GetCaptureRect());
        }

        private void OnRecordingStateChanged(RecordingSessionState state)
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

                var logicalRect = DpiHelper.ToLogical(
                    new Rect(state.ZoomArea.X, state.ZoomArea.Y, state.ZoomArea.Width, state.ZoomArea.Height),
                    _scaleFactor);
                InputFeedback.SetZoomArea(logicalRect);
                if (_settingsService.Current.EnableMinimap)
                {
                    Minimap.Update(Selection.CaptureAreaRect, InputFeedback.KeyDisplayArea);
                }
                else
                {
                    Minimap.Reset();
                }
            });
        }

        private void OnSettingsChanged(UserSettings settings)
        {
            _dispatcherQueue?.TryEnqueue(() => ApplySettings(settings));
        }

        private void ApplySettings(UserSettings settings)
        {
            EnableClickHighlight = settings.EnableClickHighlight;
            ClickHighlightColor = settings.ClickHighlightColor;
            ClickHighlightSize = settings.ClickHighlightSize;
            InputFeedback.ApplySettings(settings);
            if (!settings.EnableMinimap)
            {
                Minimap.Reset();
            }
        }
    }
}
