using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WhoIsTweeting
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private Properties.Settings appSettings = Properties.Settings.Default;

        private MainService service = (Application.Current as App).Service;

        private bool showAway, showOffline;
        private bool transparency = false;
        private bool hideBorder = false;
        private UserListItem selectedItem;

        public object userListLock = new object();

        public MainViewModel()
        {
            service.PropertyChanged += Service_PropertyChanged;
            showAway = appSettings.ShowAway;
            showOffline = appSettings.ShowOffline;
        }

        private void Service_PropertyChanged(object sender, PropertyChangedEventArgs e)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));

        public void SetConsumerKey(string consumerKey, string consumerSecret)
            => service.SetConsumerKey(consumerKey, consumerSecret);

        public void SignIn(Func<string, string> callback, Action<Exception> onError)
            => service.SignIn(callback, onError);

        public void PostTweet(string content, Action<Exception> onError)
            => service.PostTweet(content, onError);

        internal void SendDirectMessage(string screenName, string content, Action<Exception> onError)
            => service.SendDirectMessage(screenName, content, onError);

        public void TryResume()
            => service.Resume();

        public string UserMenuText
        {
            get
            {
                if (service.State == ServiceState.NeedConsumerKey)
                    return Application.Current.FindResource("Menu_Main_NeedConsumer").ToString();
                else if (service.State == ServiceState.LoginRequired)
                    return Application.Current.FindResource("Menu_Main_NeedSignIn").ToString();
                else if (service.State >= ServiceState.Ready)
                    return $"@{service.Me.screen_name}";
                else return "--";
            }
        }

        public bool CanSignIn
        {
            get
            {
                return service.State == ServiceState.LoginRequired;
            }
        }

        public ObservableCollection<UserListItem> UserList { get { return service.UserList; } }

        public UserListItem SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set
            {
                selectedItem = value;
                PropertyChanged(this, new PropertyChangedEventArgs("SelectedItem"));
            }
        }

        public int StatOnline { get { return service.OnlineCount; } }
        public int StatAway { get { return service.AwayCount; } }
        public int StatOffline { get { return service.OfflineCount; } }
        public bool StatUpdating { get { return service.State == ServiceState.Updating; } }

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

        public bool HideBorder
        {
            get { return hideBorder; }
            set
            {
                hideBorder = value;
                PropertyChanged(this, new PropertyChangedEventArgs("HideBorder"));
            }
        }

        public string ErrorMessage
        {
            get
            {
                switch (service.State)
                {
                    case ServiceState.APIError:
                        return Application.Current.FindResource("Critical_APIError").ToString();
                    case ServiceState.NetError:
                        return Application.Current.FindResource("Critical_NetError").ToString();
                    default:
                        return "";
                }
            }
        }

        public string ErrorDescription
        {
            get
            {
                switch (service.State)
                {
                    case ServiceState.APIError:
                        return Application.Current.FindResource("Critical_APIError_Description").ToString();
                    case ServiceState.NetError:
                        return Application.Current.FindResource("Critical_NetError_Description").ToString();
                    default:
                        return "";
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
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
}
