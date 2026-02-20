using System;
using System.Diagnostics;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

using ScreenOpRecorder.Features.Record;
using ScreenOpRecorder.Features.Record.State;

namespace ScreenOpRecorder.Features.Shell
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
        private readonly IRecordingDomainService _recordingDomainService;
        private readonly IRecordingStateStore _stateStore;
        private readonly Microsoft.UI.Dispatching.DispatcherQueue? _dispatcherQueue;

        private UiRecordingState _state = UiRecordingState.WaitingForSelection;
        private readonly Stopwatch _stopWatch;
        private readonly DispatcherTimer _timer;

        public event Action? StartRecord;
        public event Action? StopRecord;

        [ObservableProperty]
        public partial bool StartReady { get; set; }

        [ObservableProperty]
        public partial bool IsRecording { get; set; }

        [ObservableProperty]
        public partial string RecordingTime { get; set; } = "00:00:00";

        public ShellViewModel(ILogger<ShellViewModel> logger, IRecordingDomainService recordingDomainService, IRecordingStateStore stateStore)
        {
            _logger = logger;
            _recordingDomainService = recordingDomainService;
            _stateStore = stateStore;

            _stateStore.StateChanged += OnRecordingStateChanged;

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

            ApplyState(_stateStore.Current);
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
                var started = await _recordingDomainService.StartAsync();
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

        public async Task StopRecordingAsync()
        {
            if (_state is not (UiRecordingState.Starting or UiRecordingState.Recording))
            {
                return;
            }

            TransitionTo(UiRecordingState.Stopping);
            try
            {
                await _recordingDomainService.StopAsync();
            }
            finally
            {
                _stateStore.StateChanged -= OnRecordingStateChanged;
                _stopWatch.Stop();
                _timer.Stop();
                StopRecord?.Invoke();
                TransitionTo(UiRecordingState.WaitingForSelection);
            }
        }
        private void OnRecordingStateChanged(RecordingState state)
        {
            _dispatcherQueue?.TryEnqueue(() => ApplyState(state));
        }

        private void ApplyState(RecordingState state)
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
    }
}
