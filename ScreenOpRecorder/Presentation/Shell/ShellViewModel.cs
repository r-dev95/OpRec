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
        private readonly IHotkeyRouter _hotkeyRouter;
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

        [ObservableProperty]
        public partial TimeState TimeState { get; set; } = new();

        [ObservableProperty]
        public partial bool StartReady { get; set; }

        public event Action? StartRecord;
        public event Action? StopRecord;

        public ShellViewModel(
            ILogger<ShellViewModel> logger,
            IHotkeyRouter hotkeyRouter,
            IInputEventListener inputEventListener,
            IRecordingSessionStore stateStore,
            IStartRecordingUseCase startRecordingUseCase,
            IStopRecordingUseCase stopRecordingUseCase,
            IToggleZoomAtCursorUseCase toggleZoomAtCursorUseCase,
            IEventBus eventBus)
        {
            _logger = logger;
            _hotkeyRouter = hotkeyRouter;
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

            ChangeState(_stateStore.Current);

            _hotkeyRouter.Register(HotkeyAction.ToggleRecording, ToggleRecordingAsync);
            _hotkeyRouter.Register(HotkeyAction.ToggleZoomAtCursor, ToggleZoomAtCursorAsync);
        }

        public void Start()
        {
            if (_isStarted)
            {
                return;
            }

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

        private async void OnKeyDown(string keyName)
        {
            await _hotkeyRouter.TryHandleAsync(keyName);
        }

        private Task ToggleZoomAtCursorAsync()
        {
            return RunOnUiAsync(() =>
            {
                _toggleZoomAtCursorUseCase.TryToggle();
                return Task.CompletedTask;
            });
        }

        private Task ToggleRecordingAsync()
        {
            return RunOnUiAsync(RecordingAsync);
        }

        private Task RunOnUiAsync(Func<Task> action)
        {
            if (_dispatcherQueue != null)
            {
                var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                _dispatcherQueue.TryEnqueue(async () =>
                {
                    try
                    {
                        await action();
                        tcs.TrySetResult();
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                });
                return tcs.Task;
            }

            return action();
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

