using System;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace ScreenOpRecorder.Shared.Converters
{
    public class PointerArgsToPointConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var args = value as PointerRoutedEventArgs;
            var element = args!.OriginalSource as UIElement;
            return args!.GetCurrentPoint(element).Position;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
