using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PicoBird;
using PicoBird.Objects;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
// using System.Windows.Threading;

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
        ApiError = -1,
        NetError = -2
    };

    public class MainService : INotifyPropertyChanged, IDisposable
    {
        #region Singleton

        private static readonly Lazy<MainService> instance = new Lazy<MainService>(() => new MainService());

        public static MainService Instance => instance.Value;

        #endregion

        #region Publicly Exposed Items

        // public Dispatcher Dispatcher { get; private set; } = Dispatcher.CurrentDispatcher;

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

        public User Me { get; private set; }
        public int OnlineCount { get; private set; }
        public int AwayCount { get; private set; }
        public int OfflineCount { get; private set; }
        public int GraphCount { get; private set; } = 0;
        public int SumOnline { get; private set; } = 0;
        public int MinOnline { get; private set; } = 0;
        public int MaxOnline { get; private set; } = 0;
        public ObservableCollection<UserListItem> UserList { get; private set; }
        public ObservableCollection<KeyValuePair<DateTime, int[]>> Graph { get; private set; }

        public int UpdateInterval
        {
            get => updateInterval;
            set
            {
                appSettings.Interval = updateInterval = value;
                api.HttpTimeout = (int)(value * 0.9);
                appSettings.Save();
            }
        }

        public void SetConsumerKey(string consumerKey, string consumerSecret)
        {
            if (listUpdateWorker.IsBusy) listUpdateWorker.CancelAsync();
            appSettings.ConsumerKey = consumerKey;
            appSettings.ConsumerSecret = consumerSecret;
            appSettings.Save();

            api.ConsumerKey = consumerKey;
            api.ConsumerSecret = consumerSecret;

            State = ServiceState.SignInRequired;
        }

        public void SendDirectMessage(string screenName, string content, Action<Exception> onError)
        {
            Task postTask = api.Post("/1.1/direct_messages/new.json", null, new NameValueCollection
                {
                    { "screen_name", screenName },
                    { "text", content }
                });
            postTask.ContinueWith(task => { onError(task.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void PostTweet(string content, Action<Exception> onError)
        {
            Task postTask = api.Post("/1.1/statuses/update.json", null, new NameValueCollection
                { { "status", content } });
            postTask.ContinueWith(task => { onError(task.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void SignIn(Func<string, string> callback, Action<Exception> onError)
        {
            api.Token = api.TokenSecret = "";
            Task requestTask = api.RequestToken(url => callback(url));
            requestTask.ContinueWith(async (_) =>
            {
                if (await ValidateUser())
                {
                    appSettings.Token = api.Token;
                    appSettings.TokenSecret = api.TokenSecret;
                    appSettings.Save();
                    State = ServiceState.Ready;
                    Run();
                }
            }, TaskContinuationOptions.NotOnFaulted);
            requestTask.ContinueWith((task) =>
            {
                onError(task.Exception);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void Resume()
        {
            Task.Factory.StartNew(async () =>
            {
                if (await ValidateUser())
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

        #endregion

        private ServiceState state = ServiceState.Initial;
        private API api;
        private HashSet<string> idSet;

        private BackgroundWorker listUpdateWorker;
        private int updateInterval;

        private object userListLock = new object();
        private object graphLock = new object();

        Properties.Settings appSettings = Properties.Settings.Default;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<ErrorOccurredEventArgs> ErrorOccurred;

        private MainService()
        {
            listUpdateWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
            listUpdateWorker.DoWork += listUpdateWorker_DoWork;

            UserList = new ObservableCollection<UserListItem>();
            Graph = new ObservableCollection<KeyValuePair<DateTime, int[]>>();

            api = new API(appSettings.ConsumerKey, appSettings.ConsumerSecret)
            {
                HttpTimeout = 10,
                Token = appSettings.Token,
                TokenSecret = appSettings.TokenSecret,
                OAuthCallback = "oob"
            };
            UpdateInterval = appSettings.Interval;

            if (string.IsNullOrEmpty(api.ConsumerKey)
                || string.IsNullOrEmpty(api.ConsumerSecret))
            {
                State = ServiceState.NeedConsumerKey;
                return;
            }

            State = ServiceState.SignInRequired;
            Resume();
        }

        private async Task<bool> ValidateUser()
        {
            if (api.Token == "" || api.TokenSecret == "") return false;
            try
            {
                Me = await api.Get<User>("/1.1/account/verify_credentials.json");
                CursoredIdStrings ids = await api.Get<CursoredIdStrings>("/1.1/friends/ids.json",
                    new NameValueCollection
                    {
                        { "user_id", Me.id_str },
                        { "stringify_id", "true" },
                        { "count", "5000" }
                    });
                idSet = new HashSet<string>(ids.ids);
                return true;
            }
            catch (APIException e)
            {
                Log("MainService::ValidateUser", $"Caught APIException: {e.Message}\n{e.StackTrace}");
                State = ServiceState.ApiError;
                OnErrorOccurred("API Error", e.Message);
                return false;
            }
            catch (System.Net.Http.HttpRequestException e)
            {
                Log("MainService::ValidateUser", $"Caught HttpRequestException: {e.Message}\n{e.StackTrace}");
                State = ServiceState.NetError;
                OnErrorOccurred("Network Error", e.Message);
                return false;
            }
            catch (TaskCanceledException e)
            {
                Log("MainService::ValidateUser", $"Caught TaskCanceledException: {e.Message}\n{e.StackTrace}");
                State = ServiceState.NetError;
                OnErrorOccurred("Network Error", e.Message);
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

        private async Task UpdateUserList()
        {
            if (State < ServiceState.Ready) return;
            if (State == ServiceState.Updating) return;
            State = ServiceState.Updating;

            UserListItem.lastUpdated = DateTime.Now;
            HashSet<string> tmpSet = new HashSet<string>(idSet);
            List<UserListItem> list = new List<UserListItem>();

            try
            {
                Log("MainService::UpdateUserList", "Fetching users list");
                do
                {
                    Log("MainService::UpdateUserList", "loop");
                    HashSet<string> _ = new HashSet<string>(tmpSet.Take(100));
                    tmpSet.ExceptWith(_);
                    string data = string.Join(",", _);
                    List<User> tmp = await api.Post<List<User>>("/1.1/users/lookup.json", null, new NameValueCollection
                    {
                        { "user_id", data },
                        { "include_entities", "true" }
                    });
                    foreach (var x in tmp) list.Add(new UserListItem(x.id_str, x.name, x.screen_name, x.status));
                } while (tmpSet.Count != 0);
                Log("MainService::UpdateUserList", "Fetched users list");

                AwayCount = list.Count(x => x.Status == UserStatus.Away);
                OfflineCount = list.Count(x => x.Status == UserStatus.Offline);
                OnlineCount = idSet.Count - AwayCount - OfflineCount;

                //Dispatcher.Invoke(() =>
                //{
                    UserList.Clear();
                    foreach (var x in list) UserList.Add(x);
                //}, DispatcherPriority.Normal);

                //Dispatcher.Invoke(() =>
                //{
                    SumOnline += OnlineCount;
                    MinOnline = GraphCount == 0 ? OnlineCount : (OnlineCount < MinOnline ? OnlineCount : MinOnline);
                    MaxOnline = OnlineCount > MaxOnline ? OnlineCount : MaxOnline;
                    Graph.Add(new KeyValuePair<DateTime, int[]>(DateTime.Now, new int[] { OnlineCount, AwayCount, OfflineCount }));
                    GraphCount++;
                //}, DispatcherPriority.Normal);

                State = ServiceState.Running;
            }
            catch (APIException e)
            {
                Log("MainService::UpdateUserList", $"Caught APIException: {e.Message}\n{e.StackTrace}");
                listUpdateWorker.CancelAsync();
                State = ServiceState.ApiError;
                OnErrorOccurred("API Error", e.Message);
            }
            catch (System.Net.Http.HttpRequestException e)
            {
                Log("MainService::UpdateUserList", $"Caught HttpRequestException: {e.Message}\n{e.StackTrace}");
                listUpdateWorker.CancelAsync();
                State = ServiceState.NetError;
                OnErrorOccurred("Network Error", e.Message);
            }
            catch (TaskCanceledException e)
            {
                Log("MainService::UpdateUserList", $"Caught TaskCanceledException: {e.Message}\n{e.StackTrace}");
                listUpdateWorker.CancelAsync();
                State = ServiceState.NetError;
                OnErrorOccurred("Network Error", e.Message);
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
                    Task.Factory.StartNew(async () => await UpdateUserList());
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