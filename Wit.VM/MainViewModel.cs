using System;
using System.ComponentModel;
using System.Threading;
using Wit.Core;
using Wit.UI.Core;

namespace Wit.VM
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        public MainService Service { get; } = MainService.Instance;

        private ViewModelBase _statViewModel;

        private RelayCommand _openStatCommand;

        private const int maxRetryCount = 5;
        private double retryTimeMultiplier = 1.0;
        private bool isRetryPending = false;
        private bool isRetrying = false;
        private int retryCount = 0;
        private int retryTimeout = 0;

        private UserListItem selectedItem;
        private BackgroundWorker autoRetryWorker = new BackgroundWorker();

        public MainViewModel()
        {
            Service.PropertyChanged += Service_PropertyChanged;
            Service.ErrorOccurred += Service_ErrorOccurred;

            autoRetryWorker.DoWork += AutoRetryWorker_DoWork;
            autoRetryWorker.WorkerSupportsCancellation = true;
        }

        public MainViewModel(ViewModelFactory vmFactory, IWindowManager winManager) : this()
        {
            this.vmFactory = vmFactory;
            this.winManager = winManager;
        }

        #region Event Handlers

        private void AutoRetryWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            for (RetryTimeout = (int)(Service.UpdateInterval * retryTimeMultiplier); RetryTimeout > 0; RetryTimeout--)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(1000);
            }
            RetryCount += 1;
            retryTimeMultiplier *= 1.5;
            isRetrying = true;
            OnPropertyChanged(nameof(IsErrorSet));
            Service.Resume();
        }

        private void Service_ErrorOccurred(object sender, ErrorOccurredEventArgs e)
        {
            isRetrying = false;
            OnPropertyChanged(nameof(IsErrorSet));
            while (autoRetryWorker.IsBusy) Thread.Sleep(50); // spin-wait
            if (RetryCount >= maxRetryCount)
            {
                IsRetryPending = false;
            }
            else
            {
                IsRetryPending = true;
                autoRetryWorker.RunWorkerAsync();
            }
        }

        private void Service_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            MainService svc = sender as MainService;

            if (e.PropertyName == "State" && svc.State >= 0)
            {
                if (autoRetryWorker.IsBusy) autoRetryWorker.CancelAsync();
                RetryCount = 0;
                retryTimeMultiplier = 1.0;
                isRetrying = false;
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
            RetryCount = 0;
            retryTimeMultiplier = 1.0;
            isRetrying = true;
            OnPropertyChanged(nameof(IsErrorSet));
            Service.Resume();
        }

        public RelayCommand OpenStatCommand
        {
            get => _openStatCommand ?? (_openStatCommand = new RelayCommand(() =>
            {
                if (_statViewModel == null)
                {
                    _statViewModel = vmFactory.Create<GraphViewModel>();
                }

                winManager.ShowWindow(_statViewModel);
            }));
        }

        public bool CanSignIn => Service.State >= ServiceState.SignInRequired || Service.State == ServiceState.ApiError;

        public bool IsSignedIn => Service.State >= ServiceState.Ready;

        public bool IsErrorSet => Service.State < 0 && !isRetrying;

        public bool IsRetryPending
        {
            get => isRetryPending;
            set
            {
                isRetryPending = value;
                OnPropertyChanged(nameof(IsRetryPending));
            }
        }

        public int RetryCount
        {
            get => retryCount;
            set
            {
                retryCount = value;
                OnPropertyChanged(nameof(RetryCount));
            }
        }

        public int RetryTimeout
        {
            get => retryTimeout;
            set
            {
                retryTimeout = value;
                OnPropertyChanged(nameof(RetryTimeout));
            }
        }

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
                RefreshUserList?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool ShowOffline
        {
            get => Properties.Settings.Default.ShowOffline;
            set
            {
                Properties.Settings.Default.ShowOffline = value;
                OnPropertyChanged(nameof(ShowOffline));
                RefreshUserList?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler RefreshUserList;

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
