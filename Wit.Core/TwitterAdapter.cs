using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using PicoBird;
using PicoBird.Objects;

namespace Wit.Core
{
    class TwitterAdapter : ITwitterAdapter
    {
        API _api = new API("", "");

        public string ConsumerKey
        {
            get => _api.ConsumerKey;
            set => _api.ConsumerKey = value;
        }

        public string ConsumerSecret
        {
            get => _api.ConsumerSecret;
            set => _api.ConsumerSecret = value;
        }

        public string AccessToken
        {
            get => _api.Token;
            set => _api.Token = value;
        }

        public string AccessTokenSecret
        {
            get => _api.TokenSecret;
            set => _api.TokenSecret = value;
        }

        public TwitterApiResult<UserListItem> CheckUser()
        {
            try
            {
                User user = Task.Run(() => _api.Get<User>("/1.1/account/verify_credentials.json")).Result;

                return new UserListItem(user.id_str, user.name, user.screen_name, user.status);
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public TwitterApiResult<IEnumerable<string>> RetrieveFollowingIds(string userId)
        {
            try
            {
                CursoredIdStrings ids =
                    Task.Run(() => _api.Get<CursoredIdStrings>(
                        "/1.1/friends/ids.json",
                        new NameValueCollection
                        {
                            { "user_id", userId },
                            { "stringify_id", "true" },
                            { "count", "5000" }
                        }
                    )).Result;

                return ids.ids;
            }
            catch (Exception e)
            {
                return e;
            }
        }
    }
}
