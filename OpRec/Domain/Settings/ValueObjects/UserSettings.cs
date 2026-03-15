using OpRec.Domain.Settings.Policies;

namespace OpRec.Domain.Settings.ValueObjects
{
    public class UserSettings
    {
        public string OutputDirPath { get; set; } = UserSettingsDefaults.OutputDirPath;
        public bool OpenDirectoryAfterRecording { get; set; } = UserSettingsDefaults.OpenDirectoryAfterRecording;

        public VideoFpsOptions VideoFps { get; set; } = UserSettingsDefaults.VideoFps;
        public VideoQualityOptions VideoQuality { get; set; } = UserSettingsDefaults.VideoQuality;

        public AudioCaptureModeOptions AudioCaptureMode { get; set; } = UserSettingsDefaults.AudioCaptureMode;
        public double MicVolume { get; set; } = UserSettingsDefaults.MicVolume;
        public double SystemVolume { get; set; } = UserSettingsDefaults.SystemVolume;

        public bool EnableDoubleClickZoom { get; set; } = UserSettingsDefaults.EnableDoubleClickZoom;
        public double ZoomFactor { get; set; } = UserSettingsDefaults.ZoomFactor;
        public double ZoomInterpolationSpeed { get; set; } = UserSettingsDefaults.ZoomInterpolationSpeed;

        public bool EnableClickHighlight { get; set; } = UserSettingsDefaults.EnableClickHighlight;
        public string ClickHighlightColor { get; set; } = UserSettingsDefaults.ClickHighlightColor;
        public double ClickHighlightSize { get; set; } = UserSettingsDefaults.ClickHighlightSize;

        public bool EnableKeyDisplay { get; set; } = UserSettingsDefaults.EnableKeyDisplay;
        public KeyDisplayPositionOptions KeyDisplayPosition { get; set; } = UserSettingsDefaults.KeyDisplayPosition;
        public double KeyDisplayDurationSeconds { get; set; } = UserSettingsDefaults.KeyDisplayDurationSeconds;

        public bool EnableMinimap { get; set; } = UserSettingsDefaults.EnableMinimap;

        public string ToggleRecordingHotkey { get; set; } = UserSettingsDefaults.ToggleRecordingHotkey;
        public string ToggleZoomHotkey { get; set; } = UserSettingsDefaults.ToggleZoomHotkey;
    }
}
