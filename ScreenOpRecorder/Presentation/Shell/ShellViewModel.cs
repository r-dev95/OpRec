using System;
using System.Diagnostics;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

using ScreenOpRecorder.Core.Recording.Ports;
using ScreenOpRecorder.Core.Settings.Ports;
using ScreenOpRecorder.Core.Recording.State;
using ScreenOpRecorder.Core.Recording.UseCases;
using ScreenOpRecorder.Core.Settings.Models;

namespace ScreenOpRecorder.Presentation.Shell
{
    public partial class ShellViewModel : ObservableObject
    {
        private enum UiRecordingState
        {
            WaitingForSelection,
            ReadyToRecord,
            Starting,
            Recording,
            Stopping
        }

        private readonly ILogger<ShellViewModel> _logger;
        private readonly IRecordingCommandUseCase _recordingCommandUseCase;
        private readonly IRecordingSessionStore _stateStore;
        private readonly IUserSettingsService _settingsService;
        private readonly IGlobalKeyboardHook _keyboardHookService;
        private readonly Microsoft.UI.Dispatching.DispatcherQueue? _dispatcherQueue;

        private UiRecordingState _state = UiRecordingState.WaitingForSelection;
        private readonly Stopwatch _stopWatch;
        private readonly DispatcherTimer _timer;
        private bool _isHotkeyHandling;
        private bool _isStarted;
        private string _toggleHotkey = "CTRL+SHIFT+R";

        public event Action? StartRecord;
        public event Action? StopRecord;

        [ObservableProperty]
        public partial bool StartReady { get; set; }

        [ObservableProperty]
        public partial bool IsRecording { get; set; }

        [ObservableProperty]
        public partial string RecordingTime { get; set; } = "00:00:00";

        public ShellViewModel(
            ILogger<ShellViewModel> logger,
            IRecordingCommandUseCase recordingCommandUseCase,
            IRecordingSessionStore stateStore,
            IUserSettingsService settingsService,
            IGlobalKeyboardHook keyboardHookService)
        {
            _logger = logger;
            _recordingCommandUseCase = recordingCommandUseCase;
            _stateStore = stateStore;
            _settingsService = settingsService;
            _keyboardHookService = keyboardHookService;

            try
            {
                _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            }
            catch
            {
            }

            _stopWatch = new();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (s, e) => UpdateTime();

            ApplySettings(_settingsService.Current);
            ApplyState(_stateStore.Current);
        }

        public void Start()
        {
            if (_isStarted)
            {
                return;
            }

            _stateStore.StateChanged += OnRecordingStateChanged;
            _settingsService.SettingsChanged += OnSettingsChanged;
            _keyboardHookService.KeyDown += OnGlobalKeyDown;
            _keyboardHookService.Start();
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
            _settingsService.SettingsChanged -= OnSettingsChanged;
            _keyboardHookService.KeyDown -= OnGlobalKeyDown;
            _keyboardHookService.Stop();
            _isStarted = false;
        }

        [RelayCommand]
        private async Task RecordingAsync()
        {
            if (_state == UiRecordingState.ReadyToRecord)
            {
                await StartRecordingAsync();
                return;
            }

            if (_state == UiRecordingState.Recording)
            {
                await StopRecordingAsync();
            }
        }

        private async Task StartRecordingAsync()
        {
            if (_state != UiRecordingState.ReadyToRecord)
            {
                return;
            }

            TransitionTo(UiRecordingState.Starting);
            try
            {
                var started = await _recordingCommandUseCase.StartAsync();
                if (!started)
                {
                    TransitionTo(UiRecordingState.ReadyToRecord);
                    return;
                }

                RecordingTime = "00:00:00";
                _stopWatch.Restart();
                _timer.Start();
                StartRecord?.Invoke();
                TransitionTo(UiRecordingState.Recording);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start recording.");
                ApplyState(_stateStore.Current);
            }
        }

        private async Task StopRecordingAsync()
        {
            if (_state is not (UiRecordingState.Starting or UiRecordingState.Recording))
            {
                return;
            }

            TransitionTo(UiRecordingState.Stopping);
            try
            {
                await _recordingCommandUseCase.StopAsync();
            }
            finally
            {
                _stopWatch.Stop();
                _timer.Stop();
                StopRecord?.Invoke();
                TransitionTo(UiRecordingState.WaitingForSelection);
            }
        }

        private void OnRecordingStateChanged(RecordingSessionState state)
        {
            if (_dispatcherQueue != null)
            {
                _dispatcherQueue.TryEnqueue(() => ApplyState(state));
            }
            else
            {
                ApplyState(state);
            }
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
        }

        private async void OnGlobalKeyDown(string keyName)
        {
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

        private void ApplyState(RecordingSessionState state)
        {
            if (state.IsRecording)
            {
                if (_state is not (UiRecordingState.Recording or UiRecordingState.Starting))
                {
                    TransitionTo(UiRecordingState.Recording);
                }
                return;
            }

            if (state.HasSelection)
            {
                if (_state is not (UiRecordingState.Starting or UiRecordingState.Stopping))
                {
                    TransitionTo(UiRecordingState.ReadyToRecord);
                }
                return;
            }

            if (_state is not (UiRecordingState.Starting or UiRecordingState.Stopping))
            {
                TransitionTo(UiRecordingState.WaitingForSelection);
            }
        }

        private void UpdateTime()
        {
            var ts = _stopWatch.Elapsed;
            RecordingTime = ts.ToString(@"hh\:mm\:ss");
        }

        private void TransitionTo(UiRecordingState next)
        {
            if (_state == next)
            {
                return;
            }

            _logger.LogDebug("Shell state: {From} -> {To}", _state, next);
            _state = next;

            StartReady = next is UiRecordingState.ReadyToRecord or UiRecordingState.Recording;
            IsRecording = next is UiRecordingState.Recording or UiRecordingState.Stopping;
        }

        private static string NormalizeHotkey(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? ""
                : value.Replace(" ", "", StringComparison.Ordinal).ToUpperInvariant();
        }
    }
}
