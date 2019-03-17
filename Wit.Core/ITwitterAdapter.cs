using System.Collections.Generic;

namespace Wit.Core
{
    interface ITwitterAdapter
    {
        string ConsumerKey { get; set; }

        string ConsumerSecret { get; set; }

        string AccessToken { get; set; }

        string AccessTokenSecret { get; set; }

        TwitterApiResult<UserListItem> CheckUser();

        TwitterApiResult<IEnumerable<string>> RetrieveFollowingIds(string userId);

        TwitterApiResult<IEnumerable<UserListItem>> RetrieveFollowings(ISet<string> userIds);
    }
}