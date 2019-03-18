using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Wit.Core.Properties;

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

        #region Fields

        private readonly Settings _settings = Settings.Default;
        private readonly ITwitterAdapter _twt;
        private readonly UserListUpdater _listUpdater;
        private ServiceState _state = ServiceState.Initial;
        private HashSet<string> _userIds;

        #endregion

        #region Properties

        public ServiceState State
        {
            get => _state;
            private set
            {
                Log("MainService::State.set", $"{_state} -> {value}");
                _state = value;
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
                _settings.Interval = _listUpdater.UpdateInterval = value;
                _twt.HttpTimeout = (int)(value * 0.9);
                _settings.Save();
            }
        }

        #endregion

        #region Constructors

        static MainService()
        {
            if (Settings.Default.UpgradeSettings)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeSettings = false;
                Settings.Default.Save();
            }
        }

        private MainService()
        {
            UserList = new ObservableCollection<UserListItem>();
            Graph = new ObservableCollection<StatData>();

            _twt = new TwitterAdapter
            {
                ConsumerKey = _settings.ConsumerKey,
                ConsumerSecret = _settings.ConsumerSecret,
                AccessToken = _settings.Token,
                AccessTokenSecret = _settings.TokenSecret
            };

            _listUpdater = new UserListUpdater(_twt);
            _listUpdater.UserListUpdated += OnUserListUpdated;

            UpdateInterval = _settings.Interval;

            State =
                string.IsNullOrEmpty(_twt.ConsumerKey) || string.IsNullOrEmpty(_twt.ConsumerSecret)
                ? ServiceState.NeedConsumerKey
                : ServiceState.SignInRequired;
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler ErrorOccurred;

        #region Public Methods

        public void SetConsumerKey(string consumerKey, string consumerSecret)
        {
            _listUpdater.Stop();

            _settings.ConsumerKey = _twt.ConsumerKey = consumerKey;
            _settings.ConsumerSecret = _twt.ConsumerSecret = consumerSecret;
            _settings.Save();

            State = ServiceState.SignInRequired;
        }

        public void PostTweet(string content, Action<Exception> onError)
        {
            TwitterApiResult<bool> result = _twt.PostTweet(content);

            if (!result.DidSucceed)
            {
                onError(result.Exception);
            }
        }

        public async void SignIn(Func<string, string> callback, Action<Exception> onError)
        {
            TwitterApiResult<bool> result = await _twt.SetAccessTokenAsync(callback);

            if (result.DidSucceed && ValidateUser())
            {
                _settings.Token = _twt.AccessToken;
                _settings.TokenSecret = _twt.AccessTokenSecret;
                _settings.Save();

                State = ServiceState.Ready;
                _listUpdater.Start(_userIds);
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
                    _listUpdater.Start(_userIds);
                }
            });
        }

        public void ResetStatistics()
        {
            Graph.Clear();
            GraphCount = SumOnline = MinOnline = MaxOnline = 0;
        }

        public void Dispose() => _listUpdater.Dispose();

        #endregion

        private bool ValidateUser()
        {
            if (_twt.AccessToken == "" || _twt.AccessTokenSecret == "")
            {
                return false;
            }

            var result =
                _twt.CheckUser().Then(user =>
                {
                    Me = user;

                    return _twt.RetrieveFollowingIds(user.Id);
                });

            if (result.DidSucceed)
            {
                _userIds = new HashSet<string>(result.Data);

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

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void OnErrorOccurred()
            => ErrorOccurred?.Invoke(this, EventArgs.Empty);
    }
}