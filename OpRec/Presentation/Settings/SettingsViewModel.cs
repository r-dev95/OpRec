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
        public partial string OutputDirPath { get; set; } = UserSettingsDefaults.OutputDirPath;

        [ObservableProperty]
        public partial bool OpenDirectoryAfterRecording { get; set; } = UserSettingsDefaults.OpenDirectoryAfterRecording;

        [ObservableProperty]
        public partial VideoFpsOptions VideoFps { get; set; } = UserSettingsDefaults.VideoFps;

        [ObservableProperty]
        public partial VideoQualityOptions VideoQuality { get; set; } = UserSettingsDefaults.VideoQuality;

        [ObservableProperty]
        public partial AudioCaptureModeOptions AudioCaptureMode { get; set; } = UserSettingsDefaults.AudioCaptureMode;

        [ObservableProperty]
        public partial double MicVolume { get; set; } = UserSettingsDefaults.MicVolume;

        [ObservableProperty]
        public partial double SystemVolume { get; set; } = UserSettingsDefaults.SystemVolume;

        [ObservableProperty]
        public partial bool EnableDoubleClickZoom { get; set; } = UserSettingsDefaults.EnableDoubleClickZoom;

        [ObservableProperty]
        public partial double ZoomFactor { get; set; } = UserSettingsDefaults.ZoomFactor;

        [ObservableProperty]
        public partial double ZoomInterpolationSpeed { get; set; } = UserSettingsDefaults.ZoomInterpolationSpeed;

        [ObservableProperty]
        public partial bool EnableClickHighlight { get; set; } = UserSettingsDefaults.EnableClickHighlight;

        [ObservableProperty]
        public partial string ClickHighlightColor { get; set; } = UserSettingsDefaults.ClickHighlightColor;

        [ObservableProperty]
        public partial double ClickHighlightSize { get; set; } = UserSettingsDefaults.ClickHighlightSize;

        [ObservableProperty]
        public partial bool EnableKeyDisplay { get; set; } = UserSettingsDefaults.EnableKeyDisplay;

        [ObservableProperty]
        public partial KeyDisplayPositionOptions KeyDisplayPosition { get; set; } = UserSettingsDefaults.KeyDisplayPosition;

        [ObservableProperty]
        public partial double KeyDisplayDurationSeconds { get; set; } = UserSettingsDefaults.KeyDisplayDurationSeconds;

        [ObservableProperty]
        public partial bool EnableMinimap { get; set; } = UserSettingsDefaults.EnableMinimap;

        [ObservableProperty]
        public partial string ToggleRecordingHotkey { get; set; } = UserSettingsDefaults.ToggleRecordingHotkey;

        [ObservableProperty]
        public partial string ToggleZoomHotkey { get; set; } = UserSettingsDefaults.ToggleZoomHotkey;

        public VideoFpsOptions[] VideoFpsItems { get; } = Enum.GetValues<VideoFpsOptions>();
        public VideoQualityOptions[] VideoQualityItems { get; } = Enum.GetValues<VideoQualityOptions>();
        public KeyDisplayPositionOptions[] KeyDisplayPositionItems { get; } = Enum.GetValues<KeyDisplayPositionOptions>();
        public AudioCaptureModeOptions[] AudioCaptureModeItems { get; } = Enum.GetValues<AudioCaptureModeOptions>();

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
                OpenDirectoryAfterRecording = OpenDirectoryAfterRecording,
                VideoFps = VideoFps,
                VideoQuality = VideoQuality,
                AudioCaptureMode = AudioCaptureMode,
                MicVolume = MicVolume,
                SystemVolume = SystemVolume,
                EnableDoubleClickZoom = EnableDoubleClickZoom,
                ZoomFactor = ZoomFactor,
                ZoomInterpolationSpeed = ZoomInterpolationSpeed,
                EnableClickHighlight = EnableClickHighlight,
                ClickHighlightColor = ClickHighlightColor,
                ClickHighlightSize = ClickHighlightSize,
                EnableKeyDisplay = EnableKeyDisplay,
                KeyDisplayPosition = KeyDisplayPosition,
                KeyDisplayDurationSeconds = KeyDisplayDurationSeconds,
                EnableMinimap = EnableMinimap,
                ToggleRecordingHotkey = ToggleRecordingHotkey,
                ToggleZoomHotkey = ToggleZoomHotkey,
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
            Load(new UserSettings());
        }

        private void Load(UserSettings settings)
        {
            OutputDirPath = settings.OutputDirPath;
            OpenDirectoryAfterRecording = settings.OpenDirectoryAfterRecording;
            VideoFps = settings.VideoFps;
            VideoQuality = settings.VideoQuality;
            AudioCaptureMode = settings.AudioCaptureMode;
            MicVolume = settings.MicVolume;
            SystemVolume = settings.SystemVolume;
            EnableDoubleClickZoom = settings.EnableDoubleClickZoom;
            ZoomFactor = settings.ZoomFactor;
            ZoomInterpolationSpeed = settings.ZoomInterpolationSpeed;
            EnableClickHighlight = settings.EnableClickHighlight;
            ClickHighlightColor = settings.ClickHighlightColor;
            ClickHighlightSize = settings.ClickHighlightSize;
            EnableKeyDisplay = settings.EnableKeyDisplay;
            KeyDisplayPosition = settings.KeyDisplayPosition;
            KeyDisplayDurationSeconds = settings.KeyDisplayDurationSeconds;
            EnableMinimap = settings.EnableMinimap;
            ToggleRecordingHotkey = settings.ToggleRecordingHotkey;
            ToggleZoomHotkey = settings.ToggleZoomHotkey;
        }
    }
}
