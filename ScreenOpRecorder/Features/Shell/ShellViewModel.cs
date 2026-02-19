using System;
using System.Diagnostics;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

using ScreenOpRecorder.Features.Record;
using ScreenOpRecorder.Shared.Helpers;
using ScreenOpRecorder.Shared.Messages;

using Windows.Foundation;
using Windows.Graphics.Capture;

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
        private readonly IMessenger _messenger;
        private readonly RecordService _recordService;

        private Rect _captureArea;
        private GraphicsCaptureItem? _captureItem;
        private UiRecordingState _state = UiRecordingState.WaitingForSelection;

        private readonly Stopwatch _stopWatch;
        private readonly DispatcherTimer _timer;

        public event Action? StartRecord;
        public event Action? StopRecord;

        [ObservableProperty]
        public partial bool StartReady { get; set; } = false;

        [ObservableProperty]
        public partial bool IsRecording { get; set; } = false;

        [ObservableProperty]
        public partial string RecordingTime { get; set; } = "00:00:00";

        public ShellViewModel(ILogger<ShellViewModel> logger, IMessenger messenger, RecordService recordService)
        {
            _logger = logger;
            _messenger = messenger;
            _recordService = recordService;

            _stopWatch = new();
            _timer = new();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) => UpdateTime();

            _messenger.Register<SelectionCompletedMessage>(this, (r, m) =>
            {
                SetCaptureItem(m.captureRect);
            });

        }

        public void SetCaptureItem(Rect captureArea)
        {
            if (_state is UiRecordingState.Starting or UiRecordingState.Recording or UiRecordingState.Stopping)
            {
                return;
            }

            _captureArea = captureArea;
            _captureItem = WindowHelper.CreateForMonitor(captureArea.X, captureArea.Y, captureArea.Width, captureArea.Height);
            if (_captureItem == null)
            {
                return;
            }

            TransitionTo(UiRecordingState.ReadyToRecord);

            _logger.LogDebug("selectedRect: {} x {} - {} x {}", captureArea.X, captureArea.Y, captureArea.Width, captureArea.Height);
            _logger.LogDebug("selected item: {}, {} x {}", _captureItem.DisplayName, _captureItem.Size.Width, _captureItem.Size.Height);
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
            if (_state != UiRecordingState.ReadyToRecord || _captureItem == null)
            {
                return;
            }

            TransitionTo(UiRecordingState.Starting);

            try
            {
                _recordService.Setup(_captureItem, _captureArea);
                await _recordService.StartAsync();

                RecordingTime = "00:00:00";
                _stopWatch.Restart();
                _timer.Start();

                StartRecord?.Invoke();
                _messenger.Send(new StartRecordMessage());
                TransitionTo(UiRecordingState.Recording);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start recording.");
                TransitionTo(UiRecordingState.ReadyToRecord);
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
                await _recordService.StopAsync();
            }
            finally
            {
                _stopWatch.Stop();
                _timer.Stop();

                StopRecord?.Invoke();
                _messenger.Send(new StopRecordMessage());

                _captureItem = null;
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
