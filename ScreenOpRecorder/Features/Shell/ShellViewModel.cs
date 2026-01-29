using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Features.Record;

using Windows.Graphics.Capture;
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

            // 保存先ファイルの選択
            var filePicker = new FileSavePicker();
            filePicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            filePicker.SuggestedFileName = $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}";
            filePicker.FileTypeChoices.Add("MP4 Video", new List<string>() { ".mp4" });
            InitializeWithWindow.Initialize(filePicker, hwnd);
            var file = await filePicker.PickSaveFileAsync();
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
