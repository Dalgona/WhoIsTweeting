using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WhoIsTweeting
{
    [ValueConversion(typeof(bool), typeof(ResizeMode))]
    public class ResizeModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool)value ? ResizeMode.CanResize : ResizeMode.NoResize;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => (ResizeMode)value != ResizeMode.NoResize;
    }
}
