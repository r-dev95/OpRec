using OpRec.Domain.Settings.Policies;

namespace OpRec.Domain.Settings.ValueObjects
{
    public class UserSettings
    {
        public string OutputDirPath { get; set; } = UserSettingsConstraints.DefaultOutputDirPath;
        public bool OpenDirectoryAfterRecording { get; set; } = UserSettingsConstraints.DefaultOpenDirectoryAfterRecording;

        public int RecordingFps { get; set; } = UserSettingsConstraints.DefaultRecordingFps;
        public QualityPreset QualityPreset { get; set; } = UserSettingsConstraints.DefaultQualityPreset;

        public AudioCaptureMode AudioCaptureMode { get; set; } = UserSettingsConstraints.DefaultAudioCaptureMode;
        public double MicVolume { get; set; } = UserSettingsConstraints.DefaultMicVolume;
        public double SystemVolume { get; set; } = UserSettingsConstraints.DefaultSystemVolume;

        public bool EnableDoubleClickZoom { get; set; } = UserSettingsConstraints.DefaultEnableDoubleClickZoom;
        public double ZoomFactor { get; set; } = UserSettingsConstraints.DefaultZoomFactor;
        public double ZoomInterpolationSpeed { get; set; } = UserSettingsConstraints.DefaultZoomInterpolationSpeed;

        public bool EnableClickHighlight { get; set; } = UserSettingsConstraints.DefaultEnableClickHighlight;
        public string ClickHighlightColor { get; set; } = UserSettingsConstraints.DefaultClickHighlightColor;
        public double ClickHighlightSize { get; set; } = UserSettingsConstraints.DefaultClickHighlightSize;

        public bool EnableKeyDisplay { get; set; } = UserSettingsConstraints.DefaultEnableKeyDisplay;
        public KeyDisplayPosition KeyDisplayPosition { get; set; } = UserSettingsConstraints.DefaultKeyDisplayPosition;
        public double KeyDisplayDurationSeconds { get; set; } = UserSettingsConstraints.DefaultKeyDisplayDurationSeconds;

        public bool EnableMinimap { get; set; } = UserSettingsConstraints.DefaultEnableMinimap;

        public string ToggleRecordingHotkey { get; set; } = UserSettingsConstraints.DefaultRecordingHotkey;
        public string ToggleZoomHotkey { get; set; } = UserSettingsConstraints.DefaultZoomHotkey;
    }
}
