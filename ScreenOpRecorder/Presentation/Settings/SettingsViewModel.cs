using System;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ScreenOpRecorder.Core.Settings.Models;
using ScreenOpRecorder.Core.Settings.Ports;

namespace ScreenOpRecorder.Presentation.Settings
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly IUserSettingsService _settingsService;

        [ObservableProperty]
        public partial string OutputDirPath { get; set; } = "";

        [ObservableProperty]
        public partial int RecordingFps { get; set; } = UserSettingsConstraints.Fps30;

        [ObservableProperty]
        public partial QualityPreset QualityPreset { get; set; } = QualityPreset.High;

        [ObservableProperty]
        public partial bool EnableAudioCapture { get; set; }

        [ObservableProperty]
        public partial bool EnableClickHighlight { get; set; } = true;

        [ObservableProperty]
        public partial string ClickHighlightColor { get; set; } = UserSettingsConstraints.DefaultClickHighlightColor;

        [ObservableProperty]
        public partial double ClickHighlightSize { get; set; } = UserSettingsConstraints.DefaultClickHighlightSize;

        [ObservableProperty]
        public partial bool EnableKeyDisplay { get; set; } = true;

        [ObservableProperty]
        public partial KeyDisplayPosition KeyDisplayPosition { get; set; } = KeyDisplayPosition.BottomCenter;

        [ObservableProperty]
        public partial double KeyDisplayDurationSeconds { get; set; } = UserSettingsConstraints.DefaultKeyDisplayDurationSeconds;

        [ObservableProperty]
        public partial bool EnableMinimap { get; set; } = true;

        [ObservableProperty]
        public partial double ZoomFactor { get; set; } = UserSettingsConstraints.DefaultZoomFactor;

        [ObservableProperty]
        public partial string ToggleRecordingHotkey { get; set; } = UserSettingsConstraints.DefaultHotkey;

        [ObservableProperty]
        public partial bool OpenOutputFolderAfterRecording { get; set; }

        public int[] FpsOptions { get; } = UserSettingsConstraints.FpsOptions;
        public QualityPreset[] QualityOptions { get; } = Enum.GetValues<QualityPreset>();
        public KeyDisplayPosition[] KeyDisplayPositionOptions { get; } = Enum.GetValues<KeyDisplayPosition>();

        public event Action? CloseRequested;

        public SettingsViewModel(IUserSettingsService settingsService)
        {
            _settingsService = settingsService;
            Load(_settingsService.Current);
        }

        public void SetOutputDirPath(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                OutputDirPath = path;
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            var settings = new UserSettings
            {
                OutputDirPath = OutputDirPath,
                RecordingFps = RecordingFps,
                QualityPreset = QualityPreset,
                EnableAudioCapture = EnableAudioCapture,
                EnableClickHighlight = EnableClickHighlight,
                ClickHighlightColor = ClickHighlightColor,
                ClickHighlightSize = ClickHighlightSize,
                EnableKeyDisplay = EnableKeyDisplay,
                KeyDisplayPosition = KeyDisplayPosition,
                KeyDisplayDurationSeconds = KeyDisplayDurationSeconds,
                EnableMinimap = EnableMinimap,
                ZoomFactor = ZoomFactor,
                ToggleRecordingHotkey = ToggleRecordingHotkey,
                OpenOutputFolderAfterRecording = OpenOutputFolderAfterRecording
            };

            await _settingsService.SaveAsync(settings);
            CloseRequested?.Invoke();
        }

        [RelayCommand]
        private void Cancel()
        {
            Load(_settingsService.Current);
            CloseRequested?.Invoke();
        }

        private void Load(UserSettings settings)
        {
            OutputDirPath = settings.OutputDirPath;
            RecordingFps = settings.RecordingFps;
            QualityPreset = settings.QualityPreset;
            EnableAudioCapture = settings.EnableAudioCapture;
            EnableClickHighlight = settings.EnableClickHighlight;
            ClickHighlightColor = settings.ClickHighlightColor;
            ClickHighlightSize = settings.ClickHighlightSize;
            EnableKeyDisplay = settings.EnableKeyDisplay;
            KeyDisplayPosition = settings.KeyDisplayPosition;
            KeyDisplayDurationSeconds = settings.KeyDisplayDurationSeconds;
            EnableMinimap = settings.EnableMinimap;
            ZoomFactor = settings.ZoomFactor;
            ToggleRecordingHotkey = settings.ToggleRecordingHotkey;
            OpenOutputFolderAfterRecording = settings.OpenOutputFolderAfterRecording;
        }
    }
}
