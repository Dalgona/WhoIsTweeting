using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Wit.Core
{
    public enum ServiceState
    {
        Initial,
        NeedConsumerKey,
        SignInRequired,
        Ready,
        Running,
        Updating,
        Error = -1,
    };

    public class MainService : INotifyPropertyChanged, IDisposable
    {
        #region Singleton

        private static readonly Lazy<MainService> instance = new Lazy<MainService>(() => new MainService());

        public static MainService Instance => instance.Value;

        #endregion

        public ServiceState State
        {
            get => state;
            private set
            {
                Log("MainService::State.set", $"{state} -> {value}");
                state = value;
                OnPropertyChanged(nameof(State));
            }
        }

        public TwitterErrorType LastError { get; private set; } = TwitterErrorType.None;

        public UserListItem Me { get; private set; }
        public int OnlineCount { get; private set; }
        public int AwayCount { get; private set; }
        public int OfflineCount { get; private set; }
        public int GraphCount { get; private set; } = 0;
        public int SumOnline { get; private set; } = 0;
        public int MinOnline { get; private set; } = 0;
        public int MaxOnline { get; private set; } = 0;
        public ObservableCollection<UserListItem> UserList { get; private set; }
        public ObservableCollection<StatData> Graph { get; private set; }
        public object UserListLock { get; } = new object();
        public object GraphLock { get; } = new object();

        public int UpdateInterval
        {
            get => updateInterval;
            set
            {
                appSettings.Interval = updateInterval = value;
                _twtAdapter.HttpTimeout = (int)(value * 0.9);
                appSettings.Save();
            }
        }

        public void SetConsumerKey(string consumerKey, string consumerSecret)
        {
            if (listUpdateWorker.IsBusy) listUpdateWorker.CancelAsync();
            appSettings.ConsumerKey = consumerKey;
            appSettings.ConsumerSecret = consumerSecret;
            appSettings.Save();

            _twtAdapter.ConsumerKey = consumerKey;
            _twtAdapter.ConsumerSecret = consumerSecret;

            State = ServiceState.SignInRequired;
        }

        public void PostTweet(string content, Action<Exception> onError)
        {
            TwitterApiResult<bool> result = _twtAdapter.PostTweet(content);

            if (!result.DidSucceed)
            {
                onError(result.Exception);
            }
        }

        public async void SignIn(Func<string, string> callback, Action<Exception> onError)
        {
            TwitterApiResult<bool> result = await _twtAdapter.SetAccessTokenAsync(callback);

            if (result.DidSucceed && ValidateUser())
            {
                appSettings.Token = _twtAdapter.AccessToken;
                appSettings.TokenSecret = _twtAdapter.AccessTokenSecret;
                appSettings.Save();

                State = ServiceState.Ready;
                Run();
            }
            else
            {
                onError(result.Exception);
            }
        }

        public void Resume()
        {
            Task.Factory.StartNew(() =>
            {
                if (ValidateUser())
                {
                    State = ServiceState.Ready;
                    Run();
                }
            });
        }

        public void ResetStatistics()
        {
            Graph.Clear();
            GraphCount = SumOnline = MinOnline = MaxOnline = 0;
        }

        private ITwitterAdapter _twtAdapter;
        private ServiceState state = ServiceState.Initial;
        private HashSet<string> idSet;

        private BackgroundWorker listUpdateWorker;
        private int updateInterval;

        Properties.Settings appSettings = Properties.Settings.Default;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<ErrorOccurredEventArgs> ErrorOccurred;

        static MainService()
        {
            if (Properties.Settings.Default.UpgradeSettings)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeSettings = false;
                Properties.Settings.Default.Save();
            }
        }

        private MainService()
        {
            listUpdateWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
            listUpdateWorker.DoWork += listUpdateWorker_DoWork;

            UserList = new ObservableCollection<UserListItem>();
            Graph = new ObservableCollection<StatData>();

            _twtAdapter = new TwitterAdapter
            {
                ConsumerKey = appSettings.ConsumerKey,
                ConsumerSecret = appSettings.ConsumerSecret,
                AccessToken = appSettings.Token,
                AccessTokenSecret = appSettings.TokenSecret
            };

            UpdateInterval = appSettings.Interval;

            if (string.IsNullOrEmpty(_twtAdapter.ConsumerKey)
                || string.IsNullOrEmpty(_twtAdapter.ConsumerSecret))
            {
                State = ServiceState.NeedConsumerKey;
                return;
            }

            State = ServiceState.SignInRequired;
            Resume();
        }

        private bool ValidateUser()
        {
            if (_twtAdapter.AccessToken == "" || _twtAdapter.AccessTokenSecret == "")
            {
                return false;
            }

            var result =
                _twtAdapter.CheckUser().Then(user =>
                {
                    Me = user;

                    return _twtAdapter.RetrieveFollowingIds(user.Id);
                });

            if (result.DidSucceed)
            {
                idSet = new HashSet<string>(result.Data);

                return true;
            }
            else
            {
                LastError = result.ErrorType;
                State = ServiceState.Error;
                OnErrorOccurred(null, result.Exception.Message);

                return false;
            }
        }

        private void Run()
        {
            if (State >= ServiceState.Running) return;
            State = ServiceState.Running;
            while (listUpdateWorker.IsBusy) Thread.Sleep(50); // spin-wait
            listUpdateWorker.RunWorkerAsync();
        }

        private void UpdateUserList()
        {
            if (State < ServiceState.Ready) return;
            if (State == ServiceState.Updating) return;
            State = ServiceState.Updating;

            TwitterApiResult<IEnumerable<UserListItem>> result = _twtAdapter.RetrieveFollowings(idSet);

            if (result.DidSucceed)
            {
                IEnumerable<UserListItem> users = result.Data;

                AwayCount = users.Count(x => x.Status == UserStatus.Away);
                OfflineCount = users.Count(x => x.Status == UserStatus.Offline);
                OnlineCount = idSet.Count - AwayCount - OfflineCount;

                lock (UserListLock)
                {
                    UserList.Clear();
                    foreach (var x in users) UserList.Add(x);
                }

                lock (GraphLock)
                {
                    SumOnline += OnlineCount;
                    MinOnline = GraphCount == 0 ? OnlineCount : (OnlineCount < MinOnline ? OnlineCount : MinOnline);
                    MaxOnline = OnlineCount > MaxOnline ? OnlineCount : MaxOnline;

                    Graph.Add(new StatData(OnlineCount, AwayCount, OfflineCount));
                    GraphCount++;
                }

                State = ServiceState.Running;
            }
            else
            {
                LastError = result.ErrorType;
                State = ServiceState.Error;
                OnErrorOccurred(null, result.Exception.Message);

                listUpdateWorker.CancelAsync();
            }
        }

        private void listUpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Log("MainService::listUpdateWorker_DoWork", "called");
            BackgroundWorker worker = sender as BackgroundWorker;
            int timer = 0;
            while (true)
            {
                if (worker.CancellationPending)
                {
                    Log("MainService::listUpdateWorker_DoWork", "Worker cancellation was requested");
                    e.Cancel = true;
                    break;
                }
                if (timer % UpdateInterval == 0)
                {
                    Log("MainService::listUpdateWorker_DoWork", "Executing interval job");
                    timer = 0;
                    Task.Run((Action)UpdateUserList);
                }
                Thread.Sleep(TimeSpan.FromSeconds(0.999));
                timer++;
            }
        }

        private static void Log(string from, string message)
        {
#if DEBUG
            string now = DateTime.Now.ToString("hh:mm:ss.ffff");
            System.Diagnostics.Debug.WriteLine($"[{now}][{from}] {message}");
#endif
        }

        public void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void OnErrorOccurred(string what, string message)
            => ErrorOccurred?.Invoke(this, new ErrorOccurredEventArgs(what, message));

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    listUpdateWorker.Dispose();
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

    public class ErrorOccurredEventArgs : EventArgs
    {
        public string What { get; private set; }
        public string Message { get; private set; }

        public ErrorOccurredEventArgs(string what, string message) : base()
        {
            What = what;
            Message = message;
        }
    }
}