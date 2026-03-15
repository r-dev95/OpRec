using OpRec.Domain.Settings.Policies;

namespace OpRec.Presentation.Overlay.Recording.Models
{
    public sealed record ClickHighlightSettings(bool Enabled, string ColorHex, double Size)
    {
        public static ClickHighlightSettings Default { get; } = new(
            true,
            UserSettingsDefaults.ClickHighlightColor,
            UserSettingsDefaults.ClickHighlightSize);
    }
}
