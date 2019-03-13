using System;
using System.ComponentModel;
using System.Threading;
using Wit.Core;

namespace Wit.VM
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        public MainService Service { get; } = MainService.Instance;

        private bool transparency = false;
        private bool hideBorder = false;

        private const int maxRetryCount = 5;
        private int retryCount = 0;
        private double retryTimeMultiplier = 1.0;

        private UserListItem selectedItem;
        private BackgroundWorker autoRetryWorker = new BackgroundWorker();

        public MainViewModel()
        {
            Service.PropertyChanged += Service_PropertyChanged;
            Service.ErrorOccurred += Service_ErrorOccurred;

            autoRetryWorker.DoWork += AutoRetryWorker_DoWork;
            autoRetryWorker.WorkerSupportsCancellation = true;
        }

        #region Event Handlers

        private void AutoRetryWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            for (int i = (int)(Service.UpdateInterval * retryTimeMultiplier); i > 0; i--)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                // ErrorDescription = string.Format(Strings.Critical_AutoRetry_Message, i);
                OnPropertyChanged(nameof(ErrorDescription));
                Thread.Sleep(1000);
            }
            retryCount += 1;
            retryTimeMultiplier *= 1.5;
            Service.Resume();
        }

        private void Service_ErrorOccurred(object sender, ErrorOccurredEventArgs e)
        {
            while (autoRetryWorker.IsBusy) Thread.Sleep(50); // spin-wait
            if (retryCount >= maxRetryCount)
            {
                // ErrorDescription = string.Format(Strings.Critical_AutoRetry_Failed, maxRetryCount);
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
            => Service.SetConsumerKey(consumerKey, consumerSecret);

        public void SignIn(Func<string, string> callback, Action<Exception> onError)
            => Service.SignIn(callback, onError);

        public void PostTweet(string content, Action<Exception> onError)
            => Service.PostTweet(content, onError);

        public void SendDirectMessage(string screenName, string content, Action<Exception> onError)
            => Service.SendDirectMessage(screenName, content, onError);

        public void TryResume()
        {
            if (autoRetryWorker.IsBusy) autoRetryWorker.CancelAsync();
            retryCount = 0;
            retryTimeMultiplier = 1.0;
            Service.Resume();
        }

        public bool CanSignIn
            => Service.State >= ServiceState.SignInRequired || Service.State == ServiceState.ApiError;

        public ListCollectionView UserListView { get; private set; }

        public UserListItem SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public int StatOnline => Service.OnlineCount;
        public int StatAway => Service.AwayCount;
        public int StatOffline => Service.OfflineCount;
        public bool StatUpdating => Service.State == ServiceState.Updating;

        public bool AlwaysOnTop
        {
            get => Properties.Settings.Default.AlwaysOnTop;
            set
            {
                Properties.Settings.Default.AlwaysOnTop = value;
                OnPropertyChanged(nameof(AlwaysOnTop));
            }
        }

        public bool ShowAway
        {
            get => Properties.Settings.Default.ShowAway;
            set
            {
                Properties.Settings.Default.ShowAway = value;
                OnPropertyChanged(nameof(ShowAway));
            }
        }

        public bool ShowOffline
        {
            get => Properties.Settings.Default.ShowOffline;
            set
            {
                Properties.Settings.Default.ShowOffline = value;
                OnPropertyChanged(nameof(ShowOffline));
            }
        }

        public bool Transparency
        {
            get => transparency;
            set
            {
                transparency = value;
                OnPropertyChanged(nameof(Transparency));
            }
        }

        public bool HideBorder
        {
            get => hideBorder;
            set
            {
                hideBorder = value;
                OnPropertyChanged(nameof(HideBorder));
            }
        }

        public string ErrorDescription { get; private set; }

        private bool UserListFilter(object _item)
        {
            UserListItem item = _item as UserListItem;
            return (ShowAway || item.Status != UserStatus.Away) && (ShowOffline || item.Status != UserStatus.Offline);
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
}
