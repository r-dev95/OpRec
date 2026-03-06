using System;

using Windows.UI;

namespace ScreenOpRecorder.Presentation.Overlay.Recording.Helpers
{
    internal static class HexColorParser
    {
        public static Color ParseOrDefault(string value, Color fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            var hex = value.Trim().TrimStart('#');
            try
            {
                return hex.Length switch
                {
                    6 => Color.FromArgb(255, Convert.ToByte(hex[0..2], 16), Convert.ToByte(hex[2..4], 16), Convert.ToByte(hex[4..6], 16)),
                    8 => Color.FromArgb(Convert.ToByte(hex[0..2], 16), Convert.ToByte(hex[2..4], 16), Convert.ToByte(hex[4..6], 16), Convert.ToByte(hex[6..8], 16)),
                    _ => fallback
                };
            }
            catch
            {
                return fallback;
            }
        }
    }
}
