using OpRec.Domain.Settings.ValueObjects;

using Windows.Media.MediaProperties;

namespace OpRec.Infrastructure.Settings
{
    public static class VideoQualitySelector
    {
        public static VideoEncodingQuality FromSettings(UserSettings settings)
        {
            return ToVideoEncodingQuality(settings.VideoQuality);
        }

        public static VideoEncodingQuality ToVideoEncodingQuality(VideoQualityOptions preset)
        {
            return preset switch
            {
                VideoQualityOptions.Low => VideoEncodingQuality.Wvga,
                VideoQualityOptions.Medium => VideoEncodingQuality.HD720p,
                _ => VideoEncodingQuality.HD1080p
            };
        }
    }
}
