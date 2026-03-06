using System;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Application.Recording;
using ScreenOpRecorder.Application.Recording.Session;
using ScreenOpRecorder.Application.Settings.Ports;
using ScreenOpRecorder.Common.Helpers;
using ScreenOpRecorder.Domain.Settings.ValueObjects;
using ScreenOpRecorder.Domain.ValueObjects;

using Windows.Foundation;

namespace ScreenOpRecorder.Presentation.Overlay.Guide
{
    public partial class GuideOverlayViewModel : ObservableObject
    {
        private readonly ILogger<GuideOverlayViewModel> _logger;
        private readonly IUserSettingsService _settingsService;
        private readonly IRecordingSessionStore _stateStore;
        private readonly ISelectCaptureAreaUseCase _selectCaptureAreaUseCase;
        private readonly Microsoft.UI.Dispatching.DispatcherQueue? _dispatcherQueue;

        private double _scaleFactor = 1.0;
        private bool _isRecording;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PointerPressedCommand))]
        [NotifyCanExecuteChangedFor(nameof(PointerMovedCommand))]
        [NotifyCanExecuteChangedFor(nameof(PointerReleasedCommand))]
        public partial bool CanSubmit { get; set; }

        [ObservableProperty]
        public partial SelectionRegionState Selection { get; set; } = new();

        [ObservableProperty]
        public partial MinimapState Minimap { get; set; } = new();

        public event Action? SetRecordingUi;
        public event Action? UnSetRecordingUi;

        public GuideOverlayViewModel(
            ILogger<GuideOverlayViewModel> logger,
            IUserSettingsService settingsService,
            IRecordingSessionStore stateStore,
            ISelectCaptureAreaUseCase selectCaptureAreaUseCase)
        {
            _logger = logger;
            _settingsService = settingsService;
            _stateStore = stateStore;
            _selectCaptureAreaUseCase = selectCaptureAreaUseCase;

            try
            {
                _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            }
            catch
            {
                _logger.LogWarning("Failed to get DispatcherQueue for current thread. UI updates will be performed without dispathcher.");
            }
        }

        public void Start(double scaleFactor)
        {
            _stateStore.StateChanged += OnRecordingStateChanged;
            _settingsService.SettingsChanged += OnSettingsChanged;

            _scaleFactor = scaleFactor;
            CanSubmit = true;

            Selection.SetScaleFactor(_scaleFactor);
            Minimap.SetScaleFactor(_scaleFactor);

            ApplySettings(_settingsService.Current);
            ApplySessionState(_stateStore.Current);
        }

        public void Stop()
        {
            _stateStore.StateChanged -= OnRecordingStateChanged;
            _settingsService.SettingsChanged -= OnSettingsChanged;
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

            var physical = DpiHelper.ToPhysical(Selection.CaptureAreaRect, _scaleFactor);
            var captureRect = new ScreenRect(physical.X, physical.Y, physical.Width, physical.Height);
            _selectCaptureAreaUseCase.SelectCaptureArea(captureRect);
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

        private void OnRecordingStateChanged(RecordingSessionState state)
        {
            if (_dispatcherQueue != null)
            {
                _dispatcherQueue.TryEnqueue(() => ApplySessionState(state));
                return;
            }

            ApplySessionState(state);
        }

        private void ApplySettings(UserSettings settings)
        {
            Minimap.ApplaySettings(settings);
        }

        private void ApplySessionState(RecordingSessionState state)
        {
            if (_isRecording != state.IsRecording)
            {
                _isRecording = state.IsRecording;
                if (_isRecording)
                {
                    CanSubmit = false;
                    SetRecordingUi?.Invoke();
                }
                else
                {
                    CanSubmit = true;
                    Selection.ClearSelection();
                    UnSetRecordingUi?.Invoke();
                }
            }

            Selection.ApplySessionState(state);
            Minimap.ApplySessionState(state);
        }
    }
}
