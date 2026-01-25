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

        private GraphicsCaptureItem _selectedItem;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartRecordingCommand))]
        private bool _hasCaptureItem = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartRecordingCommand))]
        [NotifyCanExecuteChangedFor(nameof(StopRecordingCommand))]
        private bool _isRecording;

        public ShellViewModel(ILogger<ShellViewModel> logger, MainWindow mainWindow, RecordService recordService)
        {
            _logger = logger;
            _mainWindow = mainWindow;
            _recordService = recordService;
        }

        [RelayCommand]
        private async Task SelectCaptureItem()
        {
            var picker = new GraphicsCapturePicker();

            IntPtr hwnd = WindowNative.GetWindowHandle(_mainWindow);
            InitializeWithWindow.Initialize(picker, hwnd);

            _selectedItem = await picker.PickSingleItemAsync();

            if (_selectedItem != null)
            {
                // アイテムが取れたら録画ボタンを有効にする等の処理
                HasCaptureItem = true;
                _selectedItem.Closed += (s, a) => { HasCaptureItem = false; };
            }
        }

        [RelayCommand(CanExecute = nameof(CanStart))]
        private async Task StartRecordingAsync()
        {
            // 保存先ファイルの選択
            var picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            picker.FileTypeChoices.Add("MP4 Video", new List<string>() { ".mp4" });
            picker.SuggestedFileName = $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}";

            var hwnd = WindowNative.GetWindowHandle(_mainWindow);
            InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                IsRecording = true;
                _recordService.Setup(_selectedItem);
                await _recordService.StartAsync(file);
            }
        }

        [RelayCommand(CanExecute = nameof(IsRecording))]
        private async void StopRecording()
        {
            await _recordService.StopAsync();
            IsRecording = false;
        }

        private bool CanStart() => !IsRecording && HasCaptureItem;
    }
}
