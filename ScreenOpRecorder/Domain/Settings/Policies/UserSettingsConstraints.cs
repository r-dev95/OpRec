namespace ScreenOpRecorder.Domain.Settings.Policies
{
    public static class UserSettingsConstraints
    {
        public const int Fps15 = 15;
        public const int Fps30 = 30;
        public const int Fps60 = 60;
        public static readonly int[] FpsOptions = [Fps15, Fps30, Fps60];

        public const double MinKeyDisplayDurationSeconds = 0.5;
        public const double MaxKeyDisplayDurationSeconds = 10.0;
        public const double DefaultKeyDisplayDurationSeconds = 1.5;

        public const double MinZoomFactor = 1.1;
        public const double MaxZoomFactor = 4.0;
        public const double DefaultZoomFactor = 2.0;

        public const double MinClickHighlightSize = 8.0;
        public const double MaxClickHighlightSize = 120.0;
        public const double DefaultClickHighlightSize = 20.0;

        public const string DefaultHotkey = "Ctrl+Shift+R";
        public const string DefaultZoomHotkey = "Ctrl+Shift+Z";
        public const string DefaultClickHighlightColor = "#00FFFF";
    }
}
