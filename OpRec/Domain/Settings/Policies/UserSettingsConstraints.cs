using OpRec.Domain.Settings.ValueObjects;

namespace OpRec.Domain.Settings.Policies
{
    public static class UserSettingsConstraints
    {
        public const int Fps15 = 15;
        public const int Fps30 = 30;
        public const int Fps60 = 60;
        public static readonly int[] FpsOptions = [Fps15, Fps30, Fps60];

        public const string DefaultOutputDirPath = "";
        public const bool DefaultOpenDirectoryAfterRecording = false;

        public const int DefaultRecordingFps = Fps30;
        public const QualityPreset DefaultQualityPreset = QualityPreset.High;

        public const AudioCaptureMode DefaultAudioCaptureMode = AudioCaptureMode.Off;

        public const bool DefaultEnableDoubleClickZoom = true;

        public const bool DefaultEnableClickHighlight = true;
        public const string DefaultClickHighlightColor = "#00FFFF";
        public const double MinClickHighlightSize = 8.0;
        public const double MaxClickHighlightSize = 120.0;
        public const double DefaultClickHighlightSize = 20.0;

        public const double MinZoomFactor = 1.1;
        public const double MaxZoomFactor = 4.0;
        public const double DefaultZoomFactor = 2.0;
        public const double MinZoomInterpolationSpeed = 0.001;
        public const double MaxZoomInterpolationSpeed = 0.2;
        public const double DefaultZoomInterpolationSpeed = 0.01;

        public const bool DefaultEnableKeyDisplay = true;
        public const KeyDisplayPosition DefaultKeyDisplayPosition = KeyDisplayPosition.BottomCenter;
        public const double MinKeyDisplayDurationSeconds = 0.5;
        public const double MaxKeyDisplayDurationSeconds = 10.0;
        public const double DefaultKeyDisplayDurationSeconds = 1.5;

        public const bool DefaultEnableMinimap = true;

        public const string DefaultRecordingHotkey = "Ctrl+Shift+R";
        public const string DefaultZoomHotkey = "Ctrl+Shift+Z";

        public static UserSettings CreateDefaultSettings()
        {
            return new UserSettings
            {
                OutputDirPath = DefaultOutputDirPath,
                OpenDirectoryAfterRecording = DefaultOpenDirectoryAfterRecording,
                RecordingFps = DefaultRecordingFps,
                QualityPreset = DefaultQualityPreset,
                AudioCaptureMode = DefaultAudioCaptureMode,
                EnableDoubleClickZoom = DefaultEnableDoubleClickZoom,
                ZoomFactor = DefaultZoomFactor,
                ZoomInterpolationSpeed = DefaultZoomInterpolationSpeed,
                EnableClickHighlight = DefaultEnableClickHighlight,
                ClickHighlightColor = DefaultClickHighlightColor,
                ClickHighlightSize = DefaultClickHighlightSize,
                EnableKeyDisplay = DefaultEnableKeyDisplay,
                KeyDisplayPosition = DefaultKeyDisplayPosition,
                KeyDisplayDurationSeconds = DefaultKeyDisplayDurationSeconds,
                EnableMinimap = DefaultEnableMinimap,
                ToggleRecordingHotkey = DefaultRecordingHotkey,
                ToggleZoomHotkey = DefaultZoomHotkey,
            };
        }
    }
}
