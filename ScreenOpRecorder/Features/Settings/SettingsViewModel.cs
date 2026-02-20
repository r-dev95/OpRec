using System;
using System.Linq;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ScreenOpRecorder.Features.Settings
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly IUserSettingsService _settingsService;

        public event Action? CloseRequested;

        public int[] FpsOptions { get; } = [15, 30, 60];
        public QualityPreset[] QualityOptions { get; } = Enum.GetValues<QualityPreset>();
        public KeyDisplayPosition[] KeyDisplayPositionOptions { get; } = Enum.GetValues<KeyDisplayPosition>();

        [ObservableProperty]
        public partial string OutputFolderPath { get; set; } = "";

        [ObservableProperty]
        public partial int RecordingFps { get; set; } = 30;

        [ObservableProperty]
        public partial QualityPreset QualityPreset { get; set; } = QualityPreset.High;

        [ObservableProperty]
        public partial bool EnableClickHighlight { get; set; } = true;

        [ObservableProperty]
        public partial string ClickHighlightColor { get; set; } = "#00FFFF";

        [ObservableProperty]
        public partial double ClickHighlightSize { get; set; } = 20;

        [ObservableProperty]
        public partial bool EnableKeyDisplay { get; set; } = true;

        [ObservableProperty]
        public partial KeyDisplayPosition KeyDisplayPosition { get; set; } = KeyDisplayPosition.BottomCenter;

        [ObservableProperty]
        public partial double KeyDisplayDurationSeconds { get; set; } = 1.5;

        [ObservableProperty]
        public partial bool EnableMinimap { get; set; } = true;

        [ObservableProperty]
        public partial double ZoomFactor { get; set; } = 2.0;

        [ObservableProperty]
        public partial string ToggleRecordingHotkey { get; set; } = "Ctrl+Shift+R";

        [ObservableProperty]
        public partial bool OpenOutputFolderAfterRecording { get; set; }

        public SettingsViewModel(IUserSettingsService settingsService)
        {
            _settingsService = settingsService;
            Load(_settingsService.Current);
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            var fps = FpsOptions.Contains(RecordingFps) ? RecordingFps : 30;

            var settings = new UserSettings
            {
                OutputFolderPath = OutputFolderPath,
                RecordingFps = fps,
                QualityPreset = QualityPreset,
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

        public void SetOutputFolderPath(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                OutputFolderPath = path;
            }
        }

        private void Load(UserSettings settings)
        {
            OutputFolderPath = settings.OutputFolderPath;
            RecordingFps = settings.RecordingFps;
            QualityPreset = settings.QualityPreset;
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
