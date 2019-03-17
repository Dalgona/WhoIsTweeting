using System;
using System.Net.Http;
using System.Threading.Tasks;
using PicoBird;

namespace Wit.Core
{
    enum TwitterErrorType
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
