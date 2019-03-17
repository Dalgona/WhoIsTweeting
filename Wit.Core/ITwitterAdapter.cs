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

        Task<TwitterApiResult<bool>> SetAccessTokenAsync(Func<string, string> getVerifier);

        TwitterApiResult<UserListItem> CheckUser();

        TwitterApiResult<IEnumerable<string>> RetrieveFollowingIds(string userId);

        TwitterApiResult<IEnumerable<UserListItem>> RetrieveFollowings(ISet<string> userIds);

        TwitterApiResult<bool> PostTweet(string content);
    }
}