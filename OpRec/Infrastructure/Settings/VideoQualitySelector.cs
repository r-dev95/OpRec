using OpRec.Domain.Settings.ValueObjects;

namespace OpRec.Infrastructure.Settings
{
    public static class VideoQualitySelector
    {
        public static uint FromSettings(UserSettings settings)
        {
            return ToVideoBitrate(settings.VideoQuality);
        }

        public static uint ToVideoBitrate(VideoQualityOptions preset)
        {
            return preset switch
            {
                VideoQualityOptions.Low => 8_000_000,
                VideoQualityOptions.Medium => 14_000_000,
                _ => 21_000_000
            };
        }
    }
}
