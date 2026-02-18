using Microsoft.UI.Xaml;

namespace ScreenOpRecorder.Shared.Utils
{
    public static class StringUtil
    {
        public static Visibility StringToVisibility(string value)
        {
            return string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
