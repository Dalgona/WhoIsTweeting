using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
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
        private readonly RxUserListUpdater _rxListUpdater;
        private readonly StatManager _statManager = new StatManager();
        private readonly IObserver<UserListUpdaterEvent> _updaterObserver;
        private AuthStatus _authStatus = AuthStatus.NeedConsumerKey;
        private TwitterErrorType _lastError = TwitterErrorType.None;
        private UserListItem _me;
        private IDisposable _updaterSubscription;
        private bool _isUpdating = false;
        private bool _isActive = false;

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

        public TwitterErrorType LastError
        {
            get => _lastError;
            private set
            {
                _lastError = value;
                OnPropertyChanged(nameof(LastError));
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
            get => _settings.Interval;
            set
            {
                _settings.Interval = value;
                _settings.Save();

                if (_isActive)
                {
                    _updaterSubscription?.Dispose();
                    _updaterSubscription = _rxListUpdater.GetObservable(value).Subscribe(_updaterObserver);
                }
            }
        }

        public bool IsUpdating
        {
            get => _isUpdating;
            set
            {
                _isUpdating = value;
                OnPropertyChanged(nameof(IsUpdating));
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

            _rxListUpdater = new RxUserListUpdater(_twt);
            _updaterObserver = Observer.Create<UserListUpdaterEvent>(OnNext, OnError, OnCompleted);
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

        public async Task PostTweet(string content, Action<Exception> onError)
        {
            TwitterApiResult<bool> result = await _twt.PostTweet(content);

            if (!result.DidSucceed)
            {
                onError(result.Exception);
            }
        }

        public async Task SignIn(Func<string, string> callback, Action<Exception> onError)
        {
            SignOut();

            var result = await _twt.SetAccessToken(callback);

            if (!result.DidSucceed)
            {
                AuthStatus = AuthStatus.Error;
                LastError = result.ErrorType;

                onError(result.Exception);

                return;
            }

            AuthStatus = AuthStatus.Pending;
            _settings.Token = _twt.AccessToken;
            _settings.TokenSecret = _twt.AccessTokenSecret;
            _settings.Save();

            await Resume();
        }

        public async Task Resume()
        {
            if (_twt.AccessToken == "" || _twt.AccessTokenSecret == "")
            {
                return;
            }

            var userResult = await _twt.CheckUser();

            if (!userResult.DidSucceed)
            {
                AuthStatus = AuthStatus.Error;
                LastError = userResult.ErrorType;

                return;
            }

            AuthStatus = AuthStatus.OK;
            Me = userResult.Data;
            _updaterSubscription = _rxListUpdater.GetObservable(UpdateInterval).Subscribe(_updaterObserver);
        }

        public void ResetStatistics()
        {
            _statManager.Reset();
            NotifyStatChanged();
        }

        public void Dispose() => _updaterSubscription?.Dispose();

        #endregion

        private void SignOut()
        {
            _updaterSubscription?.Dispose();
            ResetStatistics();

            Me = null;
            LastError = TwitterErrorType.None;

            _settings.Token = _twt.AccessToken = "";
            _settings.TokenSecret = _twt.AccessTokenSecret = "";
            _settings.Save();

            lock (((ICollection)UserList).SyncRoot)
            {
                UserList.Clear();
            }

            AuthStatus = AuthStatus.NeedSignIn;
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

        private void OnNext(UserListUpdaterEvent item)
        {
            _isActive = true;

            switch (item)
            {
                case UpdateStartedEvent _:
                    IsUpdating = true;
                    LastError = TwitterErrorType.None;
                    break;

                case UpdateCompletedEvent completed:
                    IsUpdating = false;
                    LastError = TwitterErrorType.None;

                    lock (((ICollection)UserList).SyncRoot)
                    {
                        UserList.Clear();

                        foreach (UserListItem user in completed.Users)
                        {
                            UserList.Add(user);
                        }
                    }

                    _statManager.Update(completed.Users);
                    NotifyStatChanged();
                    break;

                case ErrorEvent error:
                    IsUpdating = false;
                    LastError = error.ErrorType;
                    break;
            }
        }

        void OnCompleted() => _isActive = false;

        void OnError(Exception _) => _isActive = false;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}