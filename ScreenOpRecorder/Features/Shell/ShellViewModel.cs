using System;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Features.Overlay;
using ScreenOpRecorder.Features.Record;

using Windows.Foundation;
using Windows.Graphics.Capture;
using Windows.Storage;

using static ScreenOpRecorder.Features.Overlay.OverlayViewModel;

namespace ScreenOpRecorder.Features.Shell
{
    public partial class ShellViewModel : ObservableObject
    {
        private readonly ILogger<ShellViewModel> _logger;
        private readonly IMessenger _messenger;
        private readonly RecordService _recordService;

        private Rect _captureArea;
        private GraphicsCaptureItem? _captureItem;

        public event Action? StartRecord;
        public event Action? StopRecord;

        public record StartRecordMessage();
        public record StopRecordMessage();

        [ObservableProperty]
        public partial bool StartReady { get; set; } = false;

        [ObservableProperty]
        public partial bool IsRecording { get; set; } = false;

        public ShellViewModel(ILogger<ShellViewModel> logger, IMessenger messenger, RecordService recordService)
        {
            _logger = logger;
            _recordService = recordService;

            _messenger = messenger;
            _messenger.Register<SelectionCompletedMessage>(this, (r, m) =>
            {
                SetCaptureItem(m.captureRect);
            });
        }

        public void SetCaptureItem(Rect captureArea)
        {
            _captureArea = captureArea;
            _captureItem = OverlayHelper.CreateForMonitor(captureArea.X, captureArea.Y, captureArea.Width, captureArea.Height);
            if (_captureItem == null)
            {
                return;
            }
            StartReady = true;

            _logger.LogDebug("selectedRect: {} x {} - {} x {}", captureArea.X, captureArea.Y, captureArea.Width, captureArea.Height);
            _logger.LogDebug("selected item: {}, {} x {}", _captureItem.DisplayName, _captureItem.Size.Width, _captureItem.Size.Height);
        }

        [RelayCommand]
        private async Task StartRecordingAsync()
        {
            if (_captureItem == null)
            {
                return;
            }

            StartRecord?.Invoke();
            _messenger.Send(new StartRecordMessage());

            // 保存先ファイルの設定
            StorageFolder localFolder = KnownFolders.VideosLibrary;
            string fileName = $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";
            StorageFile file = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            if (file != null)
            {
                IsRecording = true;
                _recordService.Setup(_captureItem, _captureArea);
                await _recordService.StartAsync(file);
            }
        }

        [RelayCommand]
        public async Task StopRecordingAsync()
        {
            await _recordService.StopAsync();
            IsRecording = false;

            StopRecord?.Invoke();
            _messenger.Send(new StopRecordMessage());
        }
    }
}
