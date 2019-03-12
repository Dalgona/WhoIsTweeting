using System;
using System.Globalization;
using System.Windows.Data;
using Wit.Core;

namespace WhoIsTweeting
{
    public class LastTweetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            UserListItem item = value as UserListItem;
            if (item?.LastTweet < new DateTime(2002, 1, 1)) return "";

            int? minutes = item?.MinutesFromLastTweet;
            if (minutes <= 5) return "";
            else if (minutes <= 15) return $"{minutes} min";
            else return minutes <= 1440 ? item?.LastTweet.ToString("HH:mm") : item?.LastTweet.ToString("yy/MM/dd");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => null;
    }
}
