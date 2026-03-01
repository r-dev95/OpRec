using ScreenOpRecorder.Application.Settings.Models;

namespace ScreenOpRecorder.Presentation.Overlay
{
    public sealed record ClickHighlightSettings(bool Enabled, string ColorHex, double Size)
    {
        public static ClickHighlightSettings Default { get; } = new(
            true,
            UserSettingsConstraints.DefaultClickHighlightColor,
            UserSettingsConstraints.DefaultClickHighlightSize);
    }
}
