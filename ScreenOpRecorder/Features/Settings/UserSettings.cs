namespace ScreenOpRecorder.Features.Settings
{
    public class UserSettings
    {
        public string OutputFolderPath { get; set; } = "";

        public int RecordingFps { get; set; } = 30;

        public QualityPreset QualityPreset { get; set; } = QualityPreset.High;

        public bool EnableClickHighlight { get; set; } = true;

        public string ClickHighlightColor { get; set; } = "#00FFFF";

        public double ClickHighlightSize { get; set; } = 20;

        public bool EnableKeyDisplay { get; set; } = true;

        public KeyDisplayPosition KeyDisplayPosition { get; set; } = KeyDisplayPosition.BottomCenter;

        public double KeyDisplayDurationSeconds { get; set; } = 1.5;

        public bool EnableMinimap { get; set; } = true;

        public double ZoomFactor { get; set; } = 2.0;

        public string ToggleRecordingHotkey { get; set; } = "Ctrl+Shift+R";

        public bool OpenOutputFolderAfterRecording { get; set; } = false;
    }
}
