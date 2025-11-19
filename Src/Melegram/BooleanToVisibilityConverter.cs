using System;
using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

#nullable disable
namespace TelegramClient
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is bool flag))
                return Visibility.Collapsed;
                
            if (parameter != null && parameter.ToString() == "Inverse")
                flag = !flag;
                
            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (!(value is Visibility visibility))
                return false;
                
            bool flag = visibility == Visibility.Visible;
            
            if (parameter != null && parameter.ToString() == "Inverse")
                flag = !flag;
                
            return flag;
        }
    }
}
