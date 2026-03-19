using System;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Extensions.Logging;

using OpRec.Application.Events.Ports;
using OpRec.Application.Recording;
using OpRec.Application.Recording.Events;
using OpRec.Application.Recording.Session;
using OpRec.Application.Settings.Ports;
using OpRec.Common.Helpers;
using OpRec.Domain.Settings.ValueObjects;
using OpRec.Domain.ValueObjects;

using Windows.Foundation;

namespace OpRec.Presentation.Overlay.Guide
{
    public partial class GuideOverlayViewModel : ObservableObject
    {
        private readonly ILogger<GuideOverlayViewModel> _logger;
        private readonly IUserSettingsService _settingsService;
        private readonly IRecordingSessionStore _stateStore;
        private readonly ISelectCaptureAreaUseCase _selectCaptureAreaUseCase;
        private readonly IEventBus _eventBus;
        private readonly Microsoft.UI.Dispatching.DispatcherQueue? _dispatcherQueue;

        private IDisposable? _startRequestSubscription;
        private double _scaleFactor = 1.0;
        private bool _isRecording;
        private bool _canSubmit;

        [ObservableProperty]
        public partial SelectionAreaState Selection { get; set; } = new();

        [ObservableProperty]
        public partial CountdownState Countdown { get; set; } = new();

        [ObservableProperty]
        public partial MinimapState Minimap { get; set; } = new();

        public event Action? SetRecordingUi;
        public event Action? UnSetRecordingUi;

        public GuideOverlayViewModel(
            ILogger<GuideOverlayViewModel> logger,
            IUserSettingsService settingsService,
            IRecordingSessionStore stateStore,
            ISelectCaptureAreaUseCase selectCaptureAreaUseCase,
            IEventBus eventBus)
        {
            _logger = logger;
            _settingsService = settingsService;
            _stateStore = stateStore;
            _selectCaptureAreaUseCase = selectCaptureAreaUseCase;
            _eventBus = eventBus;

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
            _startRequestSubscription = _eventBus.Subscribe<RecordingStartCountdownRequestedEvent>(OnRecordingStartRequestedAsync);
            _stateStore.StateChanged += OnRecordingStateChanged;
            _settingsService.SettingsChanged += OnSettingsChanged;

            _scaleFactor = scaleFactor;
            _canSubmit = true;

            Selection.SetScaleFactor(_scaleFactor);
            Selection.SetFullAreaRect(width, height);
            Minimap.SetScaleFactor(_scaleFactor);

            ApplySettings(_settingsService.Current);
            ApplySessionState(_stateStore.Current);
        }

        public void Stop()
        {
            _startRequestSubscription?.Dispose();
            _startRequestSubscription = null;
            _stateStore.StateChanged -= OnRecordingStateChanged;
            _settingsService.SettingsChanged -= OnSettingsChanged;
        }

        public void OnPointerPressed(Point position)
        {
            if (!_canSubmit)
            {
                return;
            }

            Selection.BeginSelection(position);
        }

        public void OnPointerMoved(Point currentPoint)
        {
            if (!_canSubmit)
            {
                return;
            }

            Selection.UpdateSelection(currentPoint);
        }

        public void OnPointerReleased()
        {
            if (!_canSubmit)
            {
                return;
            }

            Selection.EndSelection();
            if (!Selection.HasSelection)
            {
                return;
            }

            var physical = DpiHelper.ToPhysical(Selection.CaptureAreaRect, _scaleFactor);
            var captureRect = new ScreenRect(physical.X, physical.Y, physical.Width, physical.Height);
            _selectCaptureAreaUseCase.SelectCaptureArea(captureRect);
        }

        private async void OnRecordingStartRequestedAsync(RecordingStartCountdownRequestedEvent evt)
        {
            if (_dispatcherQueue != null)
            {
                _dispatcherQueue.TryEnqueue(async () => await RunCountdownAsync());
                return;
            }

            await RunCountdownAsync();
        }

        private async Task RunCountdownAsync()
        {
            if (Countdown.IsRunning)
            {
                return;
            }

            try
            {
                await Countdown.ShowAsync();
            }
            finally
            {
                _eventBus.Publish(new RecordingStartCountdownCompletedEvent());
            }
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
                    _canSubmit = false;
                    Selection.SetMaskVisible(false);
                    SetRecordingUi?.Invoke();
                }
                else
                {
                    _canSubmit = true;
                    Selection.SetMaskVisible(true);
                    Selection.ClearSelection();
                    UnSetRecordingUi?.Invoke();
                }
            }

            Selection.ApplySessionState(state);
            Minimap.ApplySessionState(state);
        }
    }
}
