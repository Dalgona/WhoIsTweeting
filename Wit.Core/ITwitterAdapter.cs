using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wit.Core
{
    interface ITwitterAdapter
    {
        string ConsumerKey { get; set; }
        string ConsumerSecret { get; set; }
        string AccessToken { get; set; }
        string AccessTokenSecret { get; set; }
        int HttpTimeout { get; set; }

        Task<TwitterApiResult<bool>> SetAccessTokenAsync(Func<string, string> getVerifier);
        TwitterApiResult<UserListItem> CheckUser();
        TwitterApiResult<IEnumerable<UserListItem>> RetrieveFollowings();
        TwitterApiResult<bool> PostTweet(string content);
    }
}