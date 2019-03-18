using System;
using System.ComponentModel;
using System.Threading;
using Wit.Core;
using Wit.UI.Core;

namespace Wit.VM
{
    public class MainViewModel : WindowViewModel, IDisposable
    {
        public MainService Service { get; } = MainService.Instance;

        private ViewModelBase _statViewModel;

        private RelayCommand _openStatCommand;
        private RelayCommand _openKeyCommand;
        private RelayCommand _signInCommand;
        private RelayCommand _openIntervalCommand;
        private RelayCommand _openAboutCommand;
        private RelayCommand _openMentionCommand;
        private RelayCommand _openProfileCommand;
        private RelayCommand _quitCommand;

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

            Title = "WhoIsTweeting";
            Width = 300;
            Height = 600;
            MinWidth = 300;
            MinHeight = 400;
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
            => _openStatCommand ?? (_openStatCommand = new RelayCommand(() =>
            {
                if (_statViewModel == null)
                {
                    _statViewModel = DepsInjector.Default.Create<StatViewModel>();
                }

                WindowManager.ShowWindow(_statViewModel);
            }));

        public RelayCommand OpenKeyCommand
            => _openKeyCommand ?? (_openKeyCommand = new RelayCommand(() =>
            {
                KeyViewModel vm = DepsInjector.Default.Create<KeyViewModel>();
                Core.Properties.Settings coreSettings = Core.Properties.Settings.Default;

                WindowManager.ShowModalWindow(vm, this);

                if (vm.Result && (vm.ConsumerKey != coreSettings.ConsumerKey || vm.ConsumerSecret != coreSettings.ConsumerSecret))
                {
                    Service.SetConsumerKey(vm.ConsumerKey, vm.ConsumerSecret);
                }
            }));

        public RelayCommand SignInCommand
            => _signInCommand ?? (_signInCommand = new RelayCommand(() =>
            {
                string confirmTitle = StringProvider.GetString("SignIn_Title");
                string confirmMessage = StringProvider.GetString("SignIn_Confirm");

                MessageBoxHelper.ShowInfo(confirmTitle, confirmMessage);

                Service.SignIn(url =>
                {
                    PinViewModel vm = DepsInjector.Default.Create<PinViewModel>();

                    System.Diagnostics.Process.Start(url);
                    WindowManager.ShowModalWindow(vm, this);

                    return vm.Pin;
                }, ex =>
                {
                    string errTitle = StringProvider.GetString("Title_Error");
                    string errMessage = StringProvider.GetString("SignIn_Error");

                    MessageBoxHelper.ShowError(errTitle, errMessage);
                });
            }, () => Service.State >= ServiceState.SignInRequired || Service.State == ServiceState.ApiError));

        public RelayCommand OpenIntervalCommand
            => _openIntervalCommand ?? (_openIntervalCommand = new RelayCommand(() =>
            {
                IntervalViewModel vm = DepsInjector.Default.Create<IntervalViewModel>();

                WindowManager.ShowModalWindow(vm, this);
            }));

        public RelayCommand OpenAboutCommand
            => _openAboutCommand ?? (_openAboutCommand = new RelayCommand(() =>
            {
                AboutViewModel vm = DepsInjector.Default.Create<AboutViewModel>();

                WindowManager.ShowModalWindow(vm, this);
            }));

        public RelayCommand OpenMentionCommand
            => _openMentionCommand ?? (_openMentionCommand = new RelayCommand(param =>
            {
                MentionViewModel vm = DepsInjector.Default.Create<MentionViewModel>();
                vm.User = param as UserListItem;

                WindowManager.ShowModalWindow(vm, this);

                if (vm.Result)
                {
                    Service.PostTweet(vm.Content, ex =>
                    {
                        string errTitle = StringProvider.GetString("Title_Error");
                        string errMessage = StringProvider.GetString("Message_Error_Mention");

                        MessageBoxHelper.ShowError(errTitle, errMessage);
                    });
                }
            }));

        public RelayCommand OpenProfileCommand
            => _openProfileCommand ?? (_openProfileCommand = new RelayCommand(param =>
            {
                if (param is UserListItem user)
                {
                    System.Diagnostics.Process.Start($"https://twitter.com/{user.ScreenName}");
                }
            }));

        public RelayCommand QuitCommand
            => _quitCommand ?? (_quitCommand = new RelayCommand(() => WindowManager.CloseWindow(this)));

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

        public override bool AlwaysOnTop
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
