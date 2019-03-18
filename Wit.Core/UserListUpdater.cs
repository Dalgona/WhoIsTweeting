using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Wit.Core
{
    public enum UpdaterStatus
    {
        Ready,
        Running,
        Updating,
        Error = -1
    }

    sealed class UserListUpdater : INotifyPropertyChanged, IDisposable
    {
        private UpdaterStatus _status = UpdaterStatus.Ready;
        private TwitterErrorType _lastError = TwitterErrorType.None;

        private readonly ITwitterAdapter _twtAdapter;
        private readonly BackgroundWorker _worker;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<IEnumerable<UserListItem>> UserListUpdated;
        public event EventHandler Stopped;

        public UpdaterStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public TwitterErrorType LastError
        {
            get => _lastError;
            set
            {
                _lastError = value;
                OnPropertyChanged(nameof(LastError));
            }
        }

        public int UpdateInterval { get; set; } = 10;

        public UserListUpdater(ITwitterAdapter twtAdapter)
        {
            _twtAdapter = twtAdapter;

            _worker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };

            _worker.DoWork += OnWorkerStarted;
            _worker.RunWorkerCompleted += OnWorkerCompeted;
        }

        public void Start(ISet<string> userIds)
        {
            if (Status != UpdaterStatus.Running)
            {
                Status = UpdaterStatus.Running;

                while (_worker.IsBusy)
                {
                    Thread.Sleep(50); // FIXME: naive spin-wait
                }

                _worker.RunWorkerAsync(userIds);
            }
        }

        public void Stop()
        {
            Status = UpdaterStatus.Ready;

            if (_worker.IsBusy)
            {
                _worker.CancelAsync();
            }
        }

        public void Dispose() => _worker.Dispose();

        private void UpdateUserList(BackgroundWorker worker, ISet<string> userIds)
        {
            if (Status < UpdaterStatus.Ready || Status == UpdaterStatus.Updating)
            {
                return;
            }

            Status = UpdaterStatus.Updating;

            _twtAdapter.RetrieveFollowings(userIds).Finally(users =>
            {
                UserListUpdated?.Invoke(this, users);

                Status = UpdaterStatus.Running;
            }, (errType, ex) =>
            {
                LastError = errType;
                Status = UpdaterStatus.Error;

                worker.CancelAsync();
            });
        }

        private void OnWorkerStarted(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            int timer = 0;

            while (true)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                if (timer % UpdateInterval == 0)
                {
                    timer = 0;
                    Task.Run(() => UpdateUserList(worker, e.Argument as ISet<string>));
                }

                Thread.Sleep(TimeSpan.FromSeconds(1.0));
                timer++;
            }
        }

        private void OnWorkerCompeted(object sender, RunWorkerCompletedEventArgs e)
            => Stopped?.Invoke(this, EventArgs.Empty);

        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
