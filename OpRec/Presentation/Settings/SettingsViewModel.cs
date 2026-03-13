using System;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using OpRec.Application.Settings.Ports;
using OpRec.Domain.Settings.Policies;
using OpRec.Domain.Settings.ValueObjects;

namespace OpRec.Presentation.Settings
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly IUserSettingsService _settingsService;

        [ObservableProperty]
        public partial string OutputDirPath { get; set; } = UserSettingsConstraints.DefaultOutputDirPath;

        [ObservableProperty]
        public partial int RecordingFps { get; set; } = UserSettingsConstraints.DefaultRecordingFps;

        [ObservableProperty]
        public partial QualityPreset QualityPreset { get; set; } = UserSettingsConstraints.DefaultQualityPreset;

        [ObservableProperty]
        public partial AudioCaptureMode AudioCaptureMode { get; set; } = UserSettingsConstraints.DefaultAudioCaptureMode;

        [ObservableProperty]
        public partial bool EnableClickHighlight { get; set; } = UserSettingsConstraints.DefaultEnableClickHighlight;

        [ObservableProperty]
        public partial string ClickHighlightColor { get; set; } = UserSettingsConstraints.DefaultClickHighlightColor;

        [ObservableProperty]
        public partial double ClickHighlightSize { get; set; } = UserSettingsConstraints.DefaultClickHighlightSize;

        [ObservableProperty]
        public partial bool EnableKeyDisplay { get; set; } = UserSettingsConstraints.DefaultEnableKeyDisplay;

        [ObservableProperty]
        public partial KeyDisplayPosition KeyDisplayPosition { get; set; } = UserSettingsConstraints.DefaultKeyDisplayPosition;

        [ObservableProperty]
        public partial double KeyDisplayDurationSeconds { get; set; } = UserSettingsConstraints.DefaultKeyDisplayDurationSeconds;

        [ObservableProperty]
        public partial bool EnableMinimap { get; set; } = UserSettingsConstraints.DefaultEnableMinimap;

        [ObservableProperty]
        public partial double ZoomFactor { get; set; } = UserSettingsConstraints.DefaultZoomFactor;

        [ObservableProperty]
        public partial string ToggleRecordingHotkey { get; set; } = UserSettingsConstraints.DefaultHotkey;

        [ObservableProperty]
        public partial string ToggleZoomHotkey { get; set; } = UserSettingsConstraints.DefaultZoomHotkey;

        [ObservableProperty]
        public partial bool OpenDirectoryAfterRecording { get; set; } = UserSettingsConstraints.DefaultOpenDirectoryAfterRecording;

        public int[] FpsOptions { get; } = UserSettingsConstraints.FpsOptions;
        public QualityPreset[] QualityOptions { get; } = Enum.GetValues<QualityPreset>();
        public KeyDisplayPosition[] KeyDisplayPositionOptions { get; } = Enum.GetValues<KeyDisplayPosition>();
        public AudioCaptureMode[] AudioCaptureModeOptions { get; } = Enum.GetValues<AudioCaptureMode>();

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
                AudioCaptureMode = AudioCaptureMode,
                EnableClickHighlight = EnableClickHighlight,
                ClickHighlightColor = ClickHighlightColor,
                ClickHighlightSize = ClickHighlightSize,
                EnableKeyDisplay = EnableKeyDisplay,
                KeyDisplayPosition = KeyDisplayPosition,
                KeyDisplayDurationSeconds = KeyDisplayDurationSeconds,
                EnableMinimap = EnableMinimap,
                ZoomFactor = ZoomFactor,
                ToggleRecordingHotkey = ToggleRecordingHotkey,
                ToggleZoomHotkey = ToggleZoomHotkey,
                OpenDirectoryAfterRecording = OpenDirectoryAfterRecording
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

        [RelayCommand]
        private void ResetToDefaults()
        {
            Load(UserSettingsConstraints.CreateDefaultSettings());
        }

        private void Load(UserSettings settings)
        {
            OutputDirPath = settings.OutputDirPath;
            RecordingFps = settings.RecordingFps;
            QualityPreset = settings.QualityPreset;
            AudioCaptureMode = settings.AudioCaptureMode;
            EnableClickHighlight = settings.EnableClickHighlight;
            ClickHighlightColor = settings.ClickHighlightColor;
            ClickHighlightSize = settings.ClickHighlightSize;
            EnableKeyDisplay = settings.EnableKeyDisplay;
            KeyDisplayPosition = settings.KeyDisplayPosition;
            KeyDisplayDurationSeconds = settings.KeyDisplayDurationSeconds;
            EnableMinimap = settings.EnableMinimap;
            ZoomFactor = settings.ZoomFactor;
            ToggleRecordingHotkey = settings.ToggleRecordingHotkey;
            ToggleZoomHotkey = settings.ToggleZoomHotkey;
            OpenDirectoryAfterRecording = settings.OpenDirectoryAfterRecording;
        }
    }
}
