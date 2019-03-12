using System;
using System.Globalization;
using System.Windows.Data;
using Wit.Core;

namespace WhoIsTweeting
{
    public class ServiceStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (ServiceState)value >= ServiceState.Ready;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => null;
    }
}
