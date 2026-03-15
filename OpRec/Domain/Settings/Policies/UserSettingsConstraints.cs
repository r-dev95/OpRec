namespace OpRec.Domain.Settings.Policies
{
    public static class UserSettingsConstraints
    {
        public const double MinAudioVolume = 0.0;
        public const double MaxAudioVolume = 2.0;

        public const double MinClickHighlightSize = 8.0;
        public const double MaxClickHighlightSize = 120.0;

        public const double MinZoomFactor = 1.1;
        public const double MaxZoomFactor = 4.0;
        public const double MinZoomInterpolationSpeed = 0.001;
        public const double MaxZoomInterpolationSpeed = 0.2;

        public const double MinKeyDisplayDurationSeconds = 0.5;
        public const double MaxKeyDisplayDurationSeconds = 10.0;
    }
}
