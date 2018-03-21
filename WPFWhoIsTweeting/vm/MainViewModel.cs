using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;

namespace WhoIsTweeting
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private Properties.Settings appSettings = Properties.Settings.Default;

        private MainService service = (Application.Current as App).Service;

        private bool showAway, showOffline;
        private bool transparency = false;
        private bool hideBorder = false;

        private const int maxRetryCount = 5;
        private int retryCount = 0;
        private double retryTimeMultiplier = 1.0;

        private UserListItem selectedItem;
        private BackgroundWorker autoRetryWorker = new BackgroundWorker();

        public MainViewModel()
        {
            service.PropertyChanged += Service_PropertyChanged;
            service.ErrorOccurred += Service_ErrorOccurred;

            autoRetryWorker.DoWork += AutoRetryWorker_DoWork;
            autoRetryWorker.WorkerSupportsCancellation = true;

            UserListView = new ListCollectionView(service.UserList);
            UserListView.SortDescriptions.Add(new SortDescription("MinutesFromLastTweet", ListSortDirection.Ascending));
            UserListView.GroupDescriptions.Add(new PropertyGroupDescription("Status"));
            UserListView.IsLiveFiltering = true;
            UserListView.LiveFilteringProperties.Add("Status");
            UserListView.Filter = UserListFilter;

            showAway = appSettings.ShowAway;
            showOffline = appSettings.ShowOffline;
        }

        #region Event Handlers

        private void AutoRetryWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            for (int i = (int)(service.UpdateInterval * retryTimeMultiplier); i > 0; i--)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                ErrorDescription = string.Format(Strings.Critical_AutoRetry_Message, i);
                OnPropertyChanged("ErrorDescription");
                Thread.Sleep(1000);
            }
            retryCount += 1;
            retryTimeMultiplier *= 1.5;
            service.Resume();
        }

        private void Service_ErrorOccurred(object sender, ErrorOccurredEventArgs e)
        {
            while (autoRetryWorker.IsBusy) Thread.Sleep(50); // spin-wait
            if (retryCount >= maxRetryCount)
            {
                ErrorDescription = string.Format(Strings.Critical_AutoRetry_Failed, maxRetryCount);
            }
            else
            {
                autoRetryWorker.RunWorkerAsync();
            }
        }

        private void Service_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            MainService svc = sender as MainService;

            if (e.PropertyName == "State" && svc.State >= 0)
            {
                if (autoRetryWorker.IsBusy) autoRetryWorker.CancelAsync();
                retryCount = 0;
                retryTimeMultiplier = 1.0;
            }

            OnPropertyChanged("");
        }

        #endregion

        public void SetConsumerKey(string consumerKey, string consumerSecret)
            => service.SetConsumerKey(consumerKey, consumerSecret);

        public void SignIn(Func<string, string> callback, Action<Exception> onError)
            => service.SignIn(callback, onError);

        public void PostTweet(string content, Action<Exception> onError)
            => service.PostTweet(content, onError);

        internal void SendDirectMessage(string screenName, string content, Action<Exception> onError)
            => service.SendDirectMessage(screenName, content, onError);

        public void TryResume()
        {
            if (autoRetryWorker.IsBusy) autoRetryWorker.CancelAsync();
            retryCount = 0;
            retryTimeMultiplier = 1.0;
            service.Resume();
        }

        public string UserMenuText
        {
            get
            {
                if (service.State == ServiceState.NeedConsumerKey)
                    return Strings.Menu_Main_NeedConsumer;
                else if (service.State == ServiceState.SignInRequired)
                    return Strings.Menu_Main_NeedSignIn;
                else if (service.State >= ServiceState.Ready)
                    return $"@{service.Me.screen_name}";
                else return "--";
            }
        }

        public bool CanSignIn
        {
            get
            {
                return service.State >= ServiceState.SignInRequired
                    || service.State == ServiceState.ApiError;
            }
        }

        public ListCollectionView UserListView { get; private set; }

        public UserListItem SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }

        public int StatOnline { get { return service.OnlineCount; } }
        public int StatAway { get { return service.AwayCount; } }
        public int StatOffline { get { return service.OfflineCount; } }
        public bool StatUpdating { get { return service.State == ServiceState.Updating; } }

        public bool ShowAway
        {
            get { return showAway; }
            set
            {
                Properties.Settings.Default.ShowAway = showAway = value;
                UserListView.Refresh();
            }
        }

        public bool ShowOffline
        {
            get { return showOffline; }
            set
            {
                Properties.Settings.Default.ShowOffline = showOffline = value;
                UserListView.Refresh();
            }
        }

        public bool Transparency
        {
            get { return transparency; }
            set
            {
                transparency = value;
                OnPropertyChanged("Transparency");
            }
        }

        public bool HideBorder
        {
            get { return hideBorder; }
            set
            {
                hideBorder = value;
                OnPropertyChanged("HideBorder");
            }
        }

        public string ErrorMessage
        {
            get
            {
                switch (service.State)
                {
                    case ServiceState.ApiError:
                        return Strings.Critical_ApiError;
                    case ServiceState.NetError:
                        return Strings.Critical_NetError;
                    default:
                        return "";
                }
            }
        }

        public string ErrorDescription { get; private set; }

        private bool UserListFilter(object _item)
        {
            UserListItem item = _item as UserListItem;
            return (showAway || item.Status != UserStatus.Away) && (ShowOffline || item.Status != UserStatus.Offline);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    autoRetryWorker.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
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
