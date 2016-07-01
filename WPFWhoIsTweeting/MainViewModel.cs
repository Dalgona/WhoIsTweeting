using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace WPFWhoIsTweeting
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private MainWindow window;

        private string userMenuText;
        private int statOnline, statAway, statOffline;
        private bool statUpdating;
        private bool showAway, showOffline;
        private bool transparency = false;
        private ObservableCollection<UserListItem> userList;

        public object userListLock = new object();

        public MainViewModel(MainWindow win)
        {
            window = win;
            userList = new ObservableCollection<UserListItem>();
            BindingOperations.EnableCollectionSynchronization(userList, userListLock);
        }

        public string UserMenuText
        {
            get
            {
                return userMenuText;
            }
            set
            {
                userMenuText = value;
                PropertyChanged(this, new PropertyChangedEventArgs("UserMenuText"));
            }
        }

        public ObservableCollection<UserListItem> UserList { get { return userList; } }

        public int StatOnline
        {
            get
            {
                return statOnline;
            }
            set
            {
                statOnline = value;
                PropertyChanged(this, new PropertyChangedEventArgs("StatOnline"));
            }
        }

        public int StatAway
        {
            get
            {
                return statAway;
            }
            set
            {
                statAway = value;
                PropertyChanged(this, new PropertyChangedEventArgs("StatAway"));
            }
        }

        public int StatOffline
        {
            get
            {
                return statOffline;
            }
            set
            {
                statOffline = value;
                PropertyChanged(this, new PropertyChangedEventArgs("StatOffline"));
            }
        }

        public bool StatUpdating
        {
            get
            {
                return statUpdating;
            }
            set
            {
                statUpdating = value;
                PropertyChanged(this, new PropertyChangedEventArgs("StatUpdating"));
            }
        }

        public bool ShowAway
        {
            get
            {
                return showAway;
            }
            set
            {
                Properties.Settings.Default.ShowAway = showAway = value;
                PropertyChanged(this, new PropertyChangedEventArgs("ShowAway"));
            }
        }

        public bool ShowOffline
        {
            get
            {
                return showOffline;
            }
            set
            {
                Properties.Settings.Default.ShowOffline = showOffline = value;
                PropertyChanged(this, new PropertyChangedEventArgs("ShowOffline"));
            }
        }

        public bool Transparency
        {
            get
            {
                return transparency;
            }
            set
            {
                transparency = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Transparency"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class StatCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => $"{(int)value} {parameter as string}";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => null;
    }

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

    public class TransparencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool)value ? 0.65 : 1.0;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => null;
    }

    public class ExpanderConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            System.Diagnostics.Debug.WriteLine($"'{values[0]}', '{values[1]}'");
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
