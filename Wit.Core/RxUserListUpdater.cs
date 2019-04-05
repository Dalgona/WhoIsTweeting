using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Wit.Core
{
    sealed class RxUserListUpdater
    {
        private readonly ITwitterAdapter _twt;

        public RxUserListUpdater(ITwitterAdapter twtAdapter)
        {
            _twt = twtAdapter;
        }

        public IObservable<UserListUpdaterEvent> GetObservable(int updateInterval)
        {
            _twt.HttpTimeout = (int)(updateInterval * 0.9);

            return Observable
                .Interval(TimeSpan.FromSeconds(updateInterval))
                .StartWith(0)
                .SelectMany(_ => Observable.Create<UserListUpdaterEvent>(async o =>
                {
                    o.OnNext(new UpdateStartedEvent());
                    var result = await _twt.RetrieveFollowings();

                    if (result.DidSucceed)
                    {
                        o.OnNext(new UpdateCompletedEvent(result.Data));
                    }
                    else
                    {
                        o.OnNext(new ErrorEvent(result.ErrorType, result.Exception));
                        o.OnError(result.Exception);
                    }

                    return Disposable.Empty;
                }))
                .RetryWhen(exceptions => exceptions.Delay(TimeSpan.FromSeconds(updateInterval * 1.5)));
        }
    }

    enum UserListUpdaterEventType
    {
        UpdateStarted,
        UpdateCompleted,
        Error
    }

    abstract class UserListUpdaterEvent
    {
        public abstract UserListUpdaterEventType Type { get; }
    }

    class UpdateStartedEvent : UserListUpdaterEvent
    {
        public override UserListUpdaterEventType Type => UserListUpdaterEventType.UpdateStarted;

        public override string ToString() => nameof(UpdateStartedEvent);
    }

    class UpdateCompletedEvent : UserListUpdaterEvent
    {
        public override UserListUpdaterEventType Type => UserListUpdaterEventType.UpdateCompleted;
        public IEnumerable<UserListItem> Users { get; }

        public UpdateCompletedEvent(IEnumerable<UserListItem> users) => Users = users;

        public override string ToString() => nameof(UpdateCompletedEvent);
    }

    class ErrorEvent : UserListUpdaterEvent
    {
        public override UserListUpdaterEventType Type => UserListUpdaterEventType.Error;
        public TwitterErrorType ErrorType { get; }
        public Exception Exception { get; }

        public ErrorEvent(TwitterErrorType errorType, Exception exception)
        {
            ErrorType = errorType;
            Exception = exception;
        }

        public override string ToString() => $"{nameof(ErrorEvent)}: {ErrorType} ({Exception.Message})";
    }
}
