using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Features.Record;

using Windows.Graphics.Capture;
using Windows.Services.Maps.LocalSearch;
using Windows.Storage;
using Windows.Storage.Pickers;

using WinRT.Interop;

namespace ScreenOpRecorder.Features.Shell
{
    public partial class ShellViewModel : ObservableObject
    {
        private readonly ILogger<ShellViewModel> _logger;

        private readonly MainWindow _mainWindow;
        private readonly RecordService _recordService;

        private GraphicsCaptureItem? _selectedItem;

        [ObservableProperty]
        public partial bool IsRecording { get; set; } = false;

        public ShellViewModel(ILogger<ShellViewModel> logger, MainWindow mainWindow, RecordService recordService)
        {
            _logger = logger;
            _mainWindow = mainWindow;
            _recordService = recordService;
        }

        [RelayCommand]
        private async Task StartRecordingAsync()
        {
            IntPtr hwnd = WindowNative.GetWindowHandle(_mainWindow);

            // 録画画面の選択
            var capturePicker = new GraphicsCapturePicker();
            InitializeWithWindow.Initialize(capturePicker, hwnd);
            _selectedItem = await capturePicker.PickSingleItemAsync();
            if (_selectedItem == null)
            {
                return;
            }

            // 保存先ファイルの設定
            StorageFolder localFolder = KnownFolders.VideosLibrary;
            string fileName = $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";
            StorageFile file = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            if (file != null)
            {
                IsRecording = true;
                _recordService.Setup(_selectedItem);
                await _recordService.StartAsync(file);
            }
        }

        [RelayCommand]
        private async Task StopRecordingAsync()
        {
            await _recordService.StopAsync();
            IsRecording = false;
        }
    }
}
