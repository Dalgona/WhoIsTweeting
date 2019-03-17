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

        Task<TwitterApiResult<UserListItem>> CheckUser();

        Task<TwitterApiResult<IEnumerable<string>>> RetrieveFollowingIds(string userId);
    }
}