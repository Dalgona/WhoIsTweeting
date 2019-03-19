using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private readonly StatManager _statManager = new StatManager();
        private ServiceState _state = ServiceState.Initial;

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

        public bool IsUpdating => _listUpdater.Status == UpdaterStatus.Updating;
        public UserListItem Me { get; private set; }
        public int OnlineCount => _statManager.OnlineCount;
        public int AwayCount => _statManager.AwayCount;
        public int OfflineCount => _statManager.OfflineCount;
        public int GraphCount => _statManager.DataCount;
        public int MinOnline => _statManager.MinOnline;
        public int MaxOnline => _statManager.MaxOnline;
        public double AvgOnline => _statManager.AvgOnline;
        public ObservableCollection<UserListItem> UserList { get; } = new ObservableCollection<UserListItem>();
        public ObservableCollection<StatData> Graph => _statManager.Data;
        public object UserListLock => ((ICollection)UserList).SyncRoot;
        public object GraphLock => _statManager.SyncRoot;

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
            _twt = new TwitterAdapter
            {
                ConsumerKey = _settings.ConsumerKey,
                ConsumerSecret = _settings.ConsumerSecret,
                AccessToken = _settings.Token,
                AccessTokenSecret = _settings.TokenSecret
            };

            _listUpdater = new UserListUpdater(_twt);
            _listUpdater.UserListUpdated += OnUserListUpdated;
            _listUpdater.PropertyChanged += OnUpdaterPropertyChanged;

            UpdateInterval = _settings.Interval;

            State =
                string.IsNullOrEmpty(_twt.ConsumerKey) || string.IsNullOrEmpty(_twt.ConsumerSecret)
                ? ServiceState.NeedConsumerKey
                : ServiceState.SignInRequired;
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

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
            (await _twt.SetAccessTokenAsync(callback)).Finally(async _ =>
            {
                _settings.Token = _twt.AccessToken;
                _settings.TokenSecret = _twt.AccessTokenSecret;
                _settings.Save();

                await Task.Run((Action)Resume);
            }, (errType, ex) =>
            {
                onError(ex);
            });
        }

        public void Resume()
        {
            if (_twt.AccessToken == "" || _twt.AccessTokenSecret == "")
            {
                return;
            }

            _twt.CheckUser().Then(user =>
            {
                Me = user;

                return _twt.RetrieveFollowingIds(user.Id);
            }).Finally(userIds =>
            {
                State = ServiceState.Ready;

                _listUpdater.Start(new HashSet<string>(userIds));
            }, (errType, ex) =>
            {
                LastError = errType;
                State = ServiceState.Error;
            });
        }

        public void ResetStatistics()
        {
            _statManager.Reset();
            NotifyStatChanged();
        }

        public void Dispose() => _listUpdater.Dispose();

        #endregion

        private void OnUserListUpdated(object sender, IEnumerable<UserListItem> users)
        {
            lock (UserListLock)
            {
                UserList.Clear();

                foreach (UserListItem user in users)
                {
                    UserList.Add(user);
                }
            }

            _statManager.Update(users);
            NotifyStatChanged();
        }

        private void NotifyStatChanged()
        {
            OnPropertyChanged(nameof(OnlineCount));
            OnPropertyChanged(nameof(AwayCount));
            OnPropertyChanged(nameof(OfflineCount));
            OnPropertyChanged(nameof(GraphCount));
            OnPropertyChanged(nameof(MinOnline));
            OnPropertyChanged(nameof(MaxOnline));
            OnPropertyChanged(nameof(AvgOnline));
        }

        private void OnUpdaterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UserListUpdater.Status))
            {
                OnPropertyChanged(nameof(IsUpdating));
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
    }
}