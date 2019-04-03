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

        Task<TwitterApiResult<bool>> SetAccessToken(Func<string, string> getVerifier);
        Task<TwitterApiResult<UserListItem>> CheckUser();
        Task<TwitterApiResult<IEnumerable<UserListItem>>> RetrieveFollowings();
        Task<TwitterApiResult<bool>> PostTweet(string content);
    }
}