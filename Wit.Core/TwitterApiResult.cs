using System;
using System.Net.Http;
using System.Threading.Tasks;
using PicoBird;

namespace Wit.Core
{
    public enum TwitterErrorType
    {
        None,
        NetError,
        ApiError,
        Unknown = -1
    }

    class TwitterApiResult<T>
    {
        public bool DidSucceed { get; }

        public T Data { get; }

        public TwitterErrorType ErrorType { get; }

        public Exception Exception { get; }

        private TwitterApiResult(bool isSucceeded, T data, TwitterErrorType errorType, Exception exception)
        {
            DidSucceed = isSucceeded;
            Data = data;
            ErrorType = errorType;
            Exception = exception;
        }

        public TwitterApiResult<U> Then<U>(Func<T, TwitterApiResult<U>> func)
            => DidSucceed ? func(Data) : Exception;

        public U Finally<U>(Func<T, U> onSuccess, Func<TwitterErrorType, Exception, U> onFailure)
            => DidSucceed ? onSuccess(Data) : onFailure(ErrorType, Exception);

        public void Finally(Action<T> onSuccess, Action<TwitterErrorType, Exception> onFailure)
        {
            if (DidSucceed)
            {
                onSuccess(Data);
            }
            else
            {
                onFailure(ErrorType, Exception);
            }
        }

        public static implicit operator TwitterApiResult<T>(T data)
            => new TwitterApiResult<T>(true, data, TwitterErrorType.None, null);

        public static implicit operator TwitterApiResult<T>(Exception exception)
        {
            TwitterErrorType errorType = TwitterErrorType.Unknown;

            if (exception is APIException)
            {
                errorType = TwitterErrorType.ApiError;
            }
            else if (exception is HttpRequestException)
            {
                errorType = TwitterErrorType.NetError;
            }
            else if (exception is TaskCanceledException)
            {
                errorType = TwitterErrorType.NetError;
            }

            return new TwitterApiResult<T>(false, default(T), errorType, exception);
        }
    }
}
