using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Wit.Core.Properties;

namespace Wit.Core
{
    public enum AuthStatus
    {
        OK,
        NeedConsumerKey,
        NeedSignIn,
        Pending,
        Error = -1
    }

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
        private AuthStatus _authStatus = AuthStatus.NeedConsumerKey;
        private UserListItem _me;

        #endregion

        #region Properties

        public AuthStatus AuthStatus
        {
            get => _authStatus;
            private set
            {
                _authStatus = value;
                OnPropertyChanged(nameof(AuthStatus));
            }
        }

        public UserListItem Me
        {
            get => _me;
            private set
            {
                _me = value;
                OnPropertyChanged(nameof(Me));
            }
        }

        public TwitterErrorType LastError { get; private set; } = TwitterErrorType.None;
        public bool IsUpdating => _listUpdater.Status == UpdaterStatus.Updating;
        public int OnlineCount => _statManager.OnlineCount;
        public int AwayCount => _statManager.AwayCount;
        public int OfflineCount => _statManager.OfflineCount;
        public int GraphCount => _statManager.DataCount;
        public int MinOnline => _statManager.MinOnline;
        public int MaxOnline => _statManager.MaxOnline;
        public double AvgOnline => _statManager.AvgOnline;
        public ObservableCollection<UserListItem> UserList { get; } = new ObservableCollection<UserListItem>();
        public ObservableCollection<StatData> Graph => _statManager.Data;

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

            AuthStatus =
                string.IsNullOrEmpty(_twt.ConsumerKey) || string.IsNullOrEmpty(_twt.ConsumerSecret)
                ? AuthStatus.NeedConsumerKey
                : AuthStatus.NeedSignIn;
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        #region Public Methods

        public void SetConsumerKey(string consumerKey, string consumerSecret)
        {
            SignOut();

            _settings.ConsumerKey = _twt.ConsumerKey = consumerKey;
            _settings.ConsumerSecret = _twt.ConsumerSecret = consumerSecret;
            _settings.Save();
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
            SignOut();

            (await _twt.SetAccessTokenAsync(callback)).Finally(async _ =>
            {
                AuthStatus = AuthStatus.Pending;
                _settings.Token = _twt.AccessToken;
                _settings.TokenSecret = _twt.AccessTokenSecret;
                _settings.Save();

                await Task.Run((Action)Resume);
            }, (errType, ex) =>
            {
                AuthStatus = AuthStatus.NeedSignIn;
                LastError = errType;

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
                AuthStatus = AuthStatus.OK;

                _listUpdater.Start(new HashSet<string>(userIds));
            }, (errType, ex) =>
            {
                LastError = errType;
                AuthStatus = AuthStatus.Error;
            });
        }

        public void ResetStatistics()
        {
            _statManager.Reset();
            NotifyStatChanged();
        }

        public void Dispose() => _listUpdater.Dispose();

        #endregion

        private void SignOut()
        {
            _listUpdater.Stop();
            ResetStatistics();

            Me = null;

            _settings.Token = _twt.AccessToken = "";
            _settings.TokenSecret = _twt.AccessTokenSecret = "";
            _settings.Save();

            lock (((ICollection)UserList).SyncRoot)
            {
                UserList.Clear();
            }

            AuthStatus = AuthStatus.NeedSignIn;
        }

        private void OnUserListUpdated(object sender, IEnumerable<UserListItem> users)
        {
            lock (((ICollection)UserList).SyncRoot)
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

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}