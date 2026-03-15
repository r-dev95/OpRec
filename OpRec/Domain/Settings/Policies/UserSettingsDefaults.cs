using System;

using OpRec.Domain.Settings.ValueObjects;

namespace OpRec.Domain.Settings.Policies
{
    public static class UserSettingsDefaults
    {
        public static readonly string OutputDirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        public const bool OpenDirectoryAfterRecording = false;

        public const VideoFpsOptions VideoFps = VideoFpsOptions.Fps30;
        public const VideoQualityOptions VideoQuality = VideoQualityOptions.High;

        public const AudioCaptureModeOptions AudioCaptureMode = AudioCaptureModeOptions.Off;
        public const double MicVolume = 1.0;
        public const double SystemVolume = 1.0;

        public const bool EnableDoubleClickZoom = true;
        public const double ZoomFactor = 1.5;
        public const double ZoomInterpolationSpeed = 0.02;

        public const bool EnableClickHighlight = true;
        public const string ClickHighlightColor = "#00FFFF";
        public const double ClickHighlightSize = 20.0;

        public const bool EnableKeyDisplay = true;
        public const KeyDisplayPositionOptions KeyDisplayPosition = KeyDisplayPositionOptions.BottomCenter;
        public const double KeyDisplayDurationSeconds = 1.5;

        public const bool EnableMinimap = true;

        public const string ToggleRecordingHotkey = "Ctrl+Shift+R";
        public const string ToggleZoomHotkey = "Ctrl+Shift+Z";
    }
}
