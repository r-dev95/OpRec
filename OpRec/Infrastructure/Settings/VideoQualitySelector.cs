using OpRec.Domain.Settings.ValueObjects;

using Windows.Media.MediaProperties;

namespace OpRec.Infrastructure.Settings
{
    public static class VideoQualitySelector
    {
        public static VideoEncodingQuality FromSettings(UserSettings settings)
        {
            return ToVideoEncodingQuality(settings.QualityPreset);
        }

        public static VideoEncodingQuality ToVideoEncodingQuality(QualityPreset preset)
        {
            return preset switch
            {
                QualityPreset.Low => VideoEncodingQuality.Wvga,
                QualityPreset.Medium => VideoEncodingQuality.HD720p,
                _ => VideoEncodingQuality.HD1080p
            };
        }
    }
}
