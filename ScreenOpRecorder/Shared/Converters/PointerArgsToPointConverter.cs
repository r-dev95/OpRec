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
            // SelectionCanvasに対しての相対座標を取得
            // 本来は引数でCanvasを渡すか、RelativeElementを指定する工夫が必要
            var element = args!.OriginalSource as UIElement;
            return args!.GetCurrentPoint(element).Position;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
