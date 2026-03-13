using CommunityToolkit.WinUI.Helpers;

using Windows.UI;

namespace OpRec.Presentation.Overlay.Recording.Helpers
{
    internal static class HexColorParser
    {
        public static Color ParseOrDefault(string value, Color fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            try
            {
                return value.ToColor();
            }
            catch
            {
                return fallback;
            }
        }
    }
}
