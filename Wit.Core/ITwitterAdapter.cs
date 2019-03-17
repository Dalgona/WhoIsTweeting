using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wit.Core
{
    interface ITwitterAdapter
    {
        Task<TwitterApiResult<string>> CheckUser();

        Task<TwitterApiResult<IEnumerable<string>>> RetrieveFollowingIds(string userId);
    }
}