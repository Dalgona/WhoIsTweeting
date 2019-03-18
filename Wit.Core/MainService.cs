using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
            get => _listUpdater.UpdateInterval;
            set
            {
                appSettings.Interval = _listUpdater.UpdateInterval = value;
                _twtAdapter.HttpTimeout = (int)(value * 0.9);
                appSettings.Save();
            }
        }

        public void SetConsumerKey(string consumerKey, string consumerSecret)
        {
            _listUpdater.Stop();

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

        public void Dispose() => _listUpdater.Dispose();

        private readonly ITwitterAdapter _twtAdapter;
        private readonly UserListUpdater _listUpdater;
        private ServiceState state = ServiceState.Initial;
        private HashSet<string> idSet;

        Properties.Settings appSettings = Properties.Settings.Default;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler ErrorOccurred;

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
            UserList = new ObservableCollection<UserListItem>();
            Graph = new ObservableCollection<StatData>();

            _twtAdapter = new TwitterAdapter
            {
                ConsumerKey = appSettings.ConsumerKey,
                ConsumerSecret = appSettings.ConsumerSecret,
                AccessToken = appSettings.Token,
                AccessTokenSecret = appSettings.TokenSecret
            };

            _listUpdater = new UserListUpdater(_twtAdapter);
            _listUpdater.UserListUpdated += OnUserListUpdated;

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
                OnErrorOccurred();

                return false;
            }
        }

        private void Run()
        {
            _listUpdater.Start(idSet);
        }

        private void OnUserListUpdated(object sender, IEnumerable<UserListItem> users)
        {
            var countQuery =
                from user in users
                group user by user.Status into g
                orderby g.Key
                select g.Count();

            List<int> counts = new List<int>(countQuery);
            OnlineCount = counts[0];
            AwayCount = counts[1];
            OfflineCount = counts[2];

            lock (UserListLock)
            {
                UserList.Clear();

                foreach (UserListItem user in users)
                {
                    UserList.Add(user);
                }
            }

            lock (GraphLock)
            {
                SumOnline += OnlineCount;
                MinOnline = GraphCount == 0 ? OnlineCount : (OnlineCount < MinOnline ? OnlineCount : MinOnline);
                MaxOnline = OnlineCount > MaxOnline ? OnlineCount : MaxOnline;

                Graph.Add(new StatData(OnlineCount, AwayCount, OfflineCount));
                GraphCount++;
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

        public void OnErrorOccurred()
            => ErrorOccurred?.Invoke(this, EventArgs.Empty);
    }
}