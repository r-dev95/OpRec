namespace ScreenOpRecorder.Core.Settings.Models
{
    public class UserSettings
    {
        public string OutputDirPath { get; set; } = "";

        public int RecordingFps { get; set; } = UserSettingsConstraints.Fps30;

        public QualityPreset QualityPreset { get; set; } = QualityPreset.High;

        public bool EnableAudioCapture { get; set; } = false;

        public bool EnableClickHighlight { get; set; } = true;

        public string ClickHighlightColor { get; set; } = UserSettingsConstraints.DefaultClickHighlightColor;

        public double ClickHighlightSize { get; set; } = UserSettingsConstraints.DefaultClickHighlightSize;

        public bool EnableKeyDisplay { get; set; } = true;

        public KeyDisplayPosition KeyDisplayPosition { get; set; } = KeyDisplayPosition.BottomCenter;

        public double KeyDisplayDurationSeconds { get; set; } = UserSettingsConstraints.DefaultKeyDisplayDurationSeconds;

        public bool EnableMinimap { get; set; } = true;

        public double ZoomFactor { get; set; } = UserSettingsConstraints.DefaultZoomFactor;

        public string ToggleRecordingHotkey { get; set; } = UserSettingsConstraints.DefaultHotkey;

        public bool OpenDirectoryAfterRecording { get; set; } = false;
    }
}
