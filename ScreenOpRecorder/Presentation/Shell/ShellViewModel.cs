using System;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Application.Events.Ports;
using ScreenOpRecorder.Application.Input;
using ScreenOpRecorder.Application.Recording;
using ScreenOpRecorder.Application.Recording.Events;
using ScreenOpRecorder.Application.Recording.Session;
using ScreenOpRecorder.Application.Settings.Ports;
using ScreenOpRecorder.Domain.Settings.Policies;
using ScreenOpRecorder.Domain.Settings.ValueObjects;

namespace ScreenOpRecorder.Presentation.Shell
{
    public partial class ShellViewModel : ObservableObject
    {
        private enum UiState
        {
            Waiting,
            Ready,
            Starting,
            Recording,
            Stopping
        }

        private enum PendingAction
        {
            None,
            Starting,
            Stopping
        }

        private readonly ILogger<ShellViewModel> _logger;
        private readonly IUserSettingsService _settingsService;
        private readonly IInputEventListener _inputEventListener;
        private readonly IRecordingSessionStore _stateStore;
        private readonly IStartRecordingUseCase _startRecordingUseCase;
        private readonly IStopRecordingUseCase _stopRecordingUseCase;
        private readonly IToggleZoomAtCursorUseCase _toggleZoomAtCursorUseCase;
        private readonly IEventBus _eventBus;
        private readonly Microsoft.UI.Dispatching.DispatcherQueue? _dispatcherQueue;

        private UiState _state = UiState.Waiting;
        private PendingAction _pendingAction = PendingAction.None;

        private bool _isStarted;
        private bool _isHotkeyHandling;
        private bool _isZoomHotkeyHandling;
        private string _toggleHotkey = UserSettingsConstraints.DefaultHotkey;
        private string _toggleZoomHotkey = UserSettingsConstraints.DefaultZoomHotkey;

        [ObservableProperty]
        public partial TimeState TimeState { get; set; } = new();

        [ObservableProperty]
        public partial bool StartReady { get; set; }

        public event Action? StartRecord;
        public event Action? StopRecord;

        public ShellViewModel(
            ILogger<ShellViewModel> logger,
            IUserSettingsService settingsService,
            IInputEventListener inputEventListener,
            IRecordingSessionStore stateStore,
            IStartRecordingUseCase startRecordingUseCase,
            IStopRecordingUseCase stopRecordingUseCase,
            IToggleZoomAtCursorUseCase toggleZoomAtCursorUseCase,
            IEventBus eventBus)
        {
            _logger = logger;
            _settingsService = settingsService;
            _inputEventListener = inputEventListener;
            _stateStore = stateStore;
            _startRecordingUseCase = startRecordingUseCase;
            _stopRecordingUseCase = stopRecordingUseCase;
            _toggleZoomAtCursorUseCase = toggleZoomAtCursorUseCase;
            _eventBus = eventBus;

            try
            {
                _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            }
            catch
            {
            }

            ApplySettings(_settingsService.Current);
            ChangeState(_stateStore.Current);
        }

        public void Start()
        {
            if (_isStarted)
            {
                return;
            }

            _settingsService.SettingsChanged += OnSettingsChanged;
            _stateStore.StateChanged += OnRecordingStateChanged;
            _inputEventListener.KeyDown += OnKeyDown;
            _isStarted = true;
        }

        public async Task StopAsync()
        {
            if (!_isStarted)
            {
                return;
            }

            await StopRecordingAsync();

            _settingsService.SettingsChanged -= OnSettingsChanged;
            _stateStore.StateChanged -= OnRecordingStateChanged;
            _inputEventListener.KeyDown -= OnKeyDown;
            _isStarted = false;
        }

        [RelayCommand]
        private async Task RecordingAsync()
        {
            if (_state == UiState.Ready)
            {
                await StartRecordingAsync();
                return;
            }

            if (_state == UiState.Recording)
            {
                await StopRecordingAsync();
            }
        }

        private async Task StartRecordingAsync()
        {
            if (_state != UiState.Ready)
            {
                return;
            }

            _pendingAction = PendingAction.Starting;
            ChangeState(_stateStore.Current);

            try
            {
                await WaitForStartCountdownAsync();

                var started = await _startRecordingUseCase.StartAsync();
                if (!started)
                {
                    _pendingAction = PendingAction.None;
                    ChangeState(_stateStore.Current);
                    return;
                }

                TimeState.Start();
                StartRecord?.Invoke();
            }
            catch (Exception ex)
            {
                _pendingAction = PendingAction.None;
                ChangeState(_stateStore.Current);
                _logger.LogError(ex, "Failed to start recording.");
            }
        }

        private async Task StopRecordingAsync()
        {
            if (_state is not (UiState.Starting or UiState.Recording))
            {
                return;
            }

            _pendingAction = PendingAction.Stopping;
            ChangeState(_stateStore.Current);

            try
            {
                await _stopRecordingUseCase.StopAsync();
            }
            finally
            {
                TimeState.Stop();
                StopRecord?.Invoke();
                _pendingAction = PendingAction.None;
                ChangeState(_stateStore.Current);
            }
        }

        private void OnRecordingStateChanged(RecordingSessionState state)
        {
            if (_dispatcherQueue != null)
            {
                _dispatcherQueue.TryEnqueue(() => ChangeState(state));
            }
            else
            {
                ChangeState(state);
            }
        }

        private void ChangeState(RecordingSessionState session)
        {
            UiState next;

            if (session.IsRecording)
            {
                next = _pendingAction == PendingAction.Stopping ? UiState.Stopping : UiState.Recording;
            }
            else if (session.HasSelection)
            {
                next = _pendingAction == PendingAction.Starting ? UiState.Starting : UiState.Ready;
            }
            else
            {
                next = UiState.Waiting;
            }

            if (_state == next)
            {
                return;
            }

            _logger.LogDebug("Shell UI state: {From} -> {To}", _state, next);
            _state = next;

            StartReady = next is UiState.Ready or UiState.Recording;
        }

        private void OnSettingsChanged(UserSettings settings)
        {
            if (_dispatcherQueue != null)
            {
                _dispatcherQueue.TryEnqueue(() => ApplySettings(settings));
            }
            else
            {
                ApplySettings(settings);
            }
        }

        private void ApplySettings(UserSettings settings)
        {
            _toggleHotkey = NormalizeHotkey(settings.ToggleRecordingHotkey);
            _toggleZoomHotkey = NormalizeHotkey(settings.ToggleZoomHotkey);
        }

        private async void OnKeyDown(string keyName)
        {
            if (!_isZoomHotkeyHandling && !string.IsNullOrWhiteSpace(_toggleZoomHotkey))
            {
                if (string.Equals(NormalizeHotkey(keyName), _toggleZoomHotkey, StringComparison.OrdinalIgnoreCase))
                {
                    _isZoomHotkeyHandling = true;
                    try
                    {
                        if (_dispatcherQueue != null)
                        {
                            _dispatcherQueue.TryEnqueue(ToggleZoomAtCursor);
                        }
                        else
                        {
                            ToggleZoomAtCursor();
                        }
                    }
                    finally
                    {
                        _isZoomHotkeyHandling = false;
                    }
                    return;
                }
            }

            if (_isHotkeyHandling || string.IsNullOrWhiteSpace(_toggleHotkey))
            {
                return;
            }

            if (!string.Equals(NormalizeHotkey(keyName), _toggleHotkey, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _isHotkeyHandling = true;
            try
            {
                if (_dispatcherQueue != null)
                {
                    _dispatcherQueue.TryEnqueue(async () => await RecordingAsync());
                }
                else
                {
                    await RecordingAsync();
                }
            }
            finally
            {
                _isHotkeyHandling = false;
            }
        }

        private void ToggleZoomAtCursor()
        {
            _toggleZoomAtCursorUseCase.TryToggle();
        }

        private static string NormalizeHotkey(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? ""
                : value.Replace(" ", "", StringComparison.Ordinal).ToUpperInvariant();
        }

        private async Task WaitForStartCountdownAsync()
        {
            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            IDisposable? subscription = null;

            subscription = _eventBus.Subscribe<RecordingStartCountdownCompletedEvent>(_ =>
            {
                subscription?.Dispose();
                tcs.TrySetResult();
            });

            try
            {
                _eventBus.Publish(new RecordingStartCountdownRequestedEvent());
                await tcs.Task;
            }
            finally
            {
                subscription?.Dispose();
            }
        }
    }
}

