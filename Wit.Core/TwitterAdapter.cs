using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using PicoBird;
using PicoBird.Objects;

namespace Wit.Core
{
    class TwitterAdapter : ITwitterAdapter
    {
        API _api = new API("", "");

        public async Task<TwitterApiResult<string>> CheckUser()
        {
            try
            {
                User user = await _api.Get<User>("/1.1/account/verify_credentials.json");

                return user.id_str;
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public async Task<TwitterApiResult<IEnumerable<string>>> RetrieveFollowingIds(string userId)
        {
            try
            {
                CursoredIdStrings ids =
                    await _api.Get<CursoredIdStrings>(
                        "/1.1/friends/ids.json",
                        new NameValueCollection
                        {
                            { "user_id", userId },
                            { "stringify_id", "true" },
                            { "count", "5000" }
                        }
                    );

                return ids.ids;
            }
            catch (Exception e)
            {
                return e;
            }
        }
    }
}
