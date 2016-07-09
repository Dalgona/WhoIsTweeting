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
using System.Windows.Data;

namespace WhoIsTweeting
{
    public enum ServiceState { Initial, NeedConsumerKey, LoginRequired, Ready, Running, Updating };

    public class MainService : INotifyPropertyChanged
    {
        #region Publicly Exposed Items

        public ServiceState State { get; private set; } = ServiceState.Initial;
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

        public void SetConsumerKey(string consumerKey, string consumerSecret)
        {
            if (listUpdateWorker.IsBusy) listUpdateWorker.CancelAsync();
            appSettings.ConsumerKey = consumerKey;
            appSettings.ConsumerSecret = consumerSecret;
            appSettings.Save();

            api.ConsumerKey = consumerKey;
            api.ConsumerSecret = consumerSecret;
            SetStatus(ServiceState.LoginRequired);
        }

        public void SendDirectMessage(string screenName, string content, Action<Exception> onError)
        {
            Task postTask = api.Post("/1.1/direct_messages/new.json", null, new NameValueCollection
                {
                    { "screen_name", screenName },
                    { "text", content }
                });
            Task errTask = postTask.ContinueWith(task => { onError(task.Exception); },
                TaskContinuationOptions.OnlyOnFaulted);
        }

        public void PostTweet(string content, Action<Exception> onError)
        {
            Task postTask = api.Post("/1.1/statuses/update.json", null, new NameValueCollection
                { { "status", content } });
            Task errTask = postTask.ContinueWith(task => { onError(task.Exception); },
                TaskContinuationOptions.OnlyOnFaulted);
        }

        public void SignIn(Func<string, string> callback, Action<Exception> onError)
        {
            api.Token = api.TokenSecret = "";
            Task requestTask = api.RequestToken(url => callback(url));
            Task onSuccess = requestTask.ContinueWith(async (_) =>
            {
                if (await ValidateUser())
                {
                    appSettings.Token = api.Token;
                    appSettings.TokenSecret = api.TokenSecret;
                    appSettings.Save();
                    SetStatus(ServiceState.Ready);
                    Run();
                }
            }, TaskContinuationOptions.NotOnFaulted);
            Task onFailure = requestTask.ContinueWith((task) =>
            {
                onError(task.Exception);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        #endregion

        private API api;
        private HashSet<string> idSet;

        private BackgroundWorker listUpdateWorker;

        private object userListLock = new object();
        private object graphLock = new object();

        Properties.Settings appSettings = Properties.Settings.Default;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainService()
        {
            listUpdateWorker = new BackgroundWorker();
            listUpdateWorker.DoWork += listUpdateWorker_DoWork;

            UserList = new ObservableCollection<UserListItem>();
            BindingOperations.EnableCollectionSynchronization(UserList, userListLock);
            Graph = new ObservableCollection<KeyValuePair<DateTime, int[]>>();
            BindingOperations.EnableCollectionSynchronization(Graph, graphLock);

            api = new API(appSettings.ConsumerKey, appSettings.ConsumerSecret);
            api.Token = appSettings.Token;
            api.TokenSecret = appSettings.TokenSecret;
            api.OAuthCallback = "oob";

            if (api.ConsumerKey == "" || api.ConsumerSecret == "")
            {
                SetStatus(ServiceState.NeedConsumerKey);
                return;
            }

            SetStatus(ServiceState.LoginRequired);
            Task.Factory.StartNew(async () =>
            {
                if (await ValidateUser())
                {
                    SetStatus(ServiceState.Ready);
                    Run();
                }
            });
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
                        { "stringify_id", "true" }
                    });
                idSet = new HashSet<string>(ids.ids);
                return true;
            }
            catch (APIException e)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("[ValidateUser THREW AN EXCEPTION]");
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.Info.errors[0].message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
#endif
                return false;
            }
        }

        private void SetStatus(ServiceState newStatus)
        {
            ServiceState oldStatus = State;
            switch (newStatus)
            {
                case ServiceState.Initial:
                    if (oldStatus != ServiceState.Initial)
                        throw new Exception("Invalid status change");
                    break;

                case ServiceState.NeedConsumerKey:
                    break;

                case ServiceState.LoginRequired:
                    break;

                case ServiceState.Ready:
                    if (oldStatus == ServiceState.Initial)
                        throw new Exception("Invalid status change");
                    else if (listUpdateWorker.IsBusy)
                        listUpdateWorker.CancelAsync();
                    break;

                case ServiceState.Running:
                    if (oldStatus < ServiceState.Ready)
                        throw new Exception("Invalid status change");
                    break;

                case ServiceState.Updating:
                    if (oldStatus != ServiceState.Running)
                        throw new Exception("Invalid status change");
                    break;
            }
            State = newStatus;
            OnPropertyChanged("State");
        }

        private void Run()
        {
            if (State >= ServiceState.Running) return;
            SetStatus(ServiceState.Running);
            listUpdateWorker.RunWorkerAsync();
        }

        private async Task UpdateUserList()
        {
            if (State < ServiceState.Ready) return;
            if (State == ServiceState.Updating) return;
            SetStatus(ServiceState.Updating);

            UserListItem.lastUpdated = DateTime.Now;
            HashSet<string> tmpSet = new HashSet<string>(idSet);
            List<UserListItem> list = new List<UserListItem>();

            do
            {
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

            AwayCount = list.Count(x => x.Status == UserStatus.Away);
            OfflineCount = list.Count(x => x.Status == UserStatus.Offline);
            OnlineCount = idSet.Count - AwayCount - OfflineCount;

            list.Sort((x, y) => x.MinutesFromLastTweet - y.MinutesFromLastTweet);
            lock (userListLock)
            {
                UserList.Clear();
                foreach (var x in list) UserList.Add(x);
            }

            lock (graphLock)
            {
                SumOnline += OnlineCount;
                MinOnline = GraphCount == 0 ? OnlineCount : ( OnlineCount < MinOnline ? OnlineCount : MinOnline);
                MaxOnline = OnlineCount > MaxOnline ? OnlineCount : MaxOnline;
                Graph.Add(new KeyValuePair<DateTime, int[]>(DateTime.Now, new int[] { OnlineCount, AwayCount, OfflineCount }));
                GraphCount++;
            }

            SetStatus(ServiceState.Running);
        }

        private void listUpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            while (true)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                Task t = UpdateUserList();
                Thread.Sleep(TimeSpan.FromMinutes(0.5));
            }
        }

        public void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
