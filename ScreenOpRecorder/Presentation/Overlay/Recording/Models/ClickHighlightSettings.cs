using ScreenOpRecorder.Domain.Settings.Policies;

namespace ScreenOpRecorder.Presentation.Overlay.Recording.Models
{
    public sealed record ClickHighlightSettings(bool Enabled, string ColorHex, double Size)
    {
        public static ClickHighlightSettings Default { get; } = new(
            true,
            UserSettingsConstraints.DefaultClickHighlightColor,
            UserSettingsConstraints.DefaultClickHighlightSize);
    }
}
