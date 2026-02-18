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
using Windows.Storage;

namespace ScreenOpRecorder.Features.Shell
{
    public partial class ShellViewModel : ObservableObject
    {
        private readonly ILogger<ShellViewModel> _logger;
        private readonly IMessenger _messenger;
        private readonly RecordService _recordService;

        private Rect _captureArea;
        private GraphicsCaptureItem? _captureItem;

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
            _recordService = recordService;

            _messenger = messenger;
            _messenger.Register<SelectionCompletedMessage>(this, (r, m) =>
            {
                SetCaptureItem(m.captureRect);
            });

            _stopWatch = new();
            _timer = new();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) => UpdateTime();
        }

        public void SetCaptureItem(Rect captureArea)
        {
            _captureArea = captureArea;
            _captureItem = WindowHelper.CreateForMonitor(captureArea.X, captureArea.Y, captureArea.Width, captureArea.Height);
            if (_captureItem == null)
            {
                return;
            }
            StartReady = true;

            _logger.LogDebug("selectedRect: {} x {} - {} x {}", captureArea.X, captureArea.Y, captureArea.Width, captureArea.Height);
            _logger.LogDebug("selected item: {}, {} x {}", _captureItem.DisplayName, _captureItem.Size.Width, _captureItem.Size.Height);
        }

        [RelayCommand]
        private async Task RecordingAsync()
        {
            if (!IsRecording)
            {
                await StartRecordingAsync();
            }
            else
            {
                await StopRecordingAsync();
            }
        }

        private async Task StartRecordingAsync()
        {
            if (_captureItem == null)
            {
                return;
            }

            // 保存先ファイルの設定
            StorageFolder localFolder = KnownFolders.VideosLibrary;
            string fileName = $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";
            StorageFile file = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            if (file != null)
            {
                RecordingTime = "00:00:00";
                _stopWatch.Restart();
                _timer.Start();

                IsRecording = true;

                StartRecord?.Invoke();
                _messenger.Send(new StartRecordMessage());

                _recordService.Setup(_captureItem, _captureArea);
                await _recordService.StartAsync(file);
            }
        }

        public async Task StopRecordingAsync()
        {
            await _recordService.StopAsync();

            StartReady = false;
            IsRecording = false;

            StopRecord?.Invoke();
            _messenger.Send(new StopRecordMessage());

            _stopWatch.Stop();
            _timer.Stop();
        }

        private void UpdateTime()
        {
            var ts = _stopWatch.Elapsed;
            RecordingTime = ts.ToString(@"hh\:mm\:ss");
        }
    }
}
