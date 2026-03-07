using System;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Application.Input;
using ScreenOpRecorder.Application.Recording.Session;
using ScreenOpRecorder.Application.Settings.Ports;
using ScreenOpRecorder.Common.Helpers;
using ScreenOpRecorder.Domain.Settings.ValueObjects;
using ScreenOpRecorder.Presentation.Overlay.Recording.Models;

using Windows.Foundation;

namespace ScreenOpRecorder.Presentation.Overlay.Recording
{
    public partial class RecordingOverlayViewModel : ObservableObject
    {
        private readonly ILogger<RecordingOverlayViewModel> _logger;
        private readonly IUserSettingsService _settingsService;
        private readonly IInputEventListener _inputEventListener;
        private readonly IRecordingSessionStore _stateStore;
        private readonly Microsoft.UI.Dispatching.DispatcherQueue? _dispatcherQueue;

        private double _scaleFactor = 1.0;
        private bool _isRecording;

        [ObservableProperty]
        public partial InputFeedbackState InputFeedback { get; set; } = new();

        public ClickHighlightSettings ClickHighlight = ClickHighlightSettings.Default;

        public event Action? SetRecordingUi;
        public event Action? UnSetRecordingUi;
        public event Action<double, double, bool>? ClickHighlightRequested;

        public RecordingOverlayViewModel(
            ILogger<RecordingOverlayViewModel> logger,
            IUserSettingsService settingsService,
            IInputEventListener inputEventListener,
            IRecordingSessionStore stateStore)
        {
            _logger = logger;
            _settingsService = settingsService;
            _inputEventListener = inputEventListener;
            _stateStore = stateStore;

            try
            {
                _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            }
            catch
            {
                _logger.LogWarning("Failed to get DispatcherQueue for current thread. UI updates will be performed without dispathcher.");
            }
        }

        public void Start(double scaleFactor, double width, double height)
        {
            _inputEventListener.MouseClicked += OnMouseClicked;
            _inputEventListener.KeyDown += OnKeyDown;
            _stateStore.StateChanged += OnRecordingStateChanged;
            _settingsService.SettingsChanged += OnSettingsChanged;

            _scaleFactor = scaleFactor;

            InputFeedback.SetScaleFactor(_scaleFactor);
            InputFeedback.SetScreenSize(width, height);

            ApplySettings(_settingsService.Current);
            ApplySessionState(_stateStore.Current);
        }

        public void Stop()
        {
            _inputEventListener.MouseClicked -= OnMouseClicked;
            _inputEventListener.KeyDown -= OnKeyDown;
            _stateStore.StateChanged -= OnRecordingStateChanged;
            _settingsService.SettingsChanged -= OnSettingsChanged;
        }

        private void OnMouseClicked(int x, int y, bool isDouble)
        {
            var logicalPoint = DpiHelper.ToLogical(new Point(x, y), _scaleFactor);
            ClickHighlightRequested?.Invoke(logicalPoint.X, logicalPoint.Y, isDouble);
        }

        private async void OnKeyDown(string keyName)
        {
            await InputFeedback.ShowKeyAsync(keyName);
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
            ClickHighlight = new ClickHighlightSettings(
                settings.EnableClickHighlight,
                settings.ClickHighlightColor,
                settings.ClickHighlightSize);
            InputFeedback.ApplySettings(settings);
        }

        private void ApplySessionState(RecordingSessionState state)
        {
            if (_isRecording != state.IsRecording)
            {
                _isRecording = state.IsRecording;
                if (_isRecording)
                {
                    SetRecordingUi?.Invoke();
                }
                else
                {
                    UnSetRecordingUi?.Invoke();
                }
            }

            InputFeedback.ApplySessionState(state);
        }
    }
}
