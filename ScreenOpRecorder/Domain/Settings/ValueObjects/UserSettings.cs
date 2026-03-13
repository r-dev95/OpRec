using ScreenOpRecorder.Domain.Settings.Policies;

namespace ScreenOpRecorder.Domain.Settings.ValueObjects
{
    public class UserSettings
    {
        public string OutputDirPath { get; set; } = UserSettingsConstraints.DefaultOutputDirPath;

        public int RecordingFps { get; set; } = UserSettingsConstraints.DefaultRecordingFps;

        public QualityPreset QualityPreset { get; set; } = UserSettingsConstraints.DefaultQualityPreset;

        public AudioCaptureMode AudioCaptureMode { get; set; } = UserSettingsConstraints.DefaultAudioCaptureMode;

        public bool EnableClickHighlight { get; set; } = UserSettingsConstraints.DefaultEnableClickHighlight;

        public string ClickHighlightColor { get; set; } = UserSettingsConstraints.DefaultClickHighlightColor;

        public double ClickHighlightSize { get; set; } = UserSettingsConstraints.DefaultClickHighlightSize;

        public bool EnableKeyDisplay { get; set; } = UserSettingsConstraints.DefaultEnableKeyDisplay;

        public KeyDisplayPosition KeyDisplayPosition { get; set; } = UserSettingsConstraints.DefaultKeyDisplayPosition;

        public double KeyDisplayDurationSeconds { get; set; } = UserSettingsConstraints.DefaultKeyDisplayDurationSeconds;

        public bool EnableMinimap { get; set; } = UserSettingsConstraints.DefaultEnableMinimap;

        public double ZoomFactor { get; set; } = UserSettingsConstraints.DefaultZoomFactor;

        public string ToggleRecordingHotkey { get; set; } = UserSettingsConstraints.DefaultHotkey;

        public string ToggleZoomHotkey { get; set; } = UserSettingsConstraints.DefaultZoomHotkey;

        public bool OpenDirectoryAfterRecording { get; set; } = UserSettingsConstraints.DefaultOpenDirectoryAfterRecording;
    }
}
