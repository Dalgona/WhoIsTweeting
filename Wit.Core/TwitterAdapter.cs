using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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

        public TwitterApiResult<IEnumerable<UserListItem>> RetrieveFollowings(ISet<string> userIds)
        {
            UserListItem.lastUpdated = DateTime.Now;
            HashSet<string> userIdsCopy = new HashSet<string>(userIds);
            List<UserListItem> list = new List<UserListItem>();

            try
            {
                do
                {
                    HashSet<string> batch = new HashSet<string>(userIdsCopy.Take(100));
                    string data = string.Join(",", batch);

                    List<User> tmp =
                        Task.Run(() => _api.Post<List<User>>("/1.1/users/lookup.json", null, new NameValueCollection
                        {
                            { "user_id", data },
                            { "include_entities", "true" }
                        })).Result;

                    list.AddRange(from u in tmp select new UserListItem(u.id_str, u.name, u.screen_name, u.status));
                    userIdsCopy.ExceptWith(batch);
                } while (userIdsCopy.Count != 0);

                return list;
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public TwitterApiResult<bool> PostTweet(string content)
        {
            try
            {
                Task.Run(() => _api.Post("/1.1/statuses/update.json", null, new NameValueCollection
                {
                    { "status", content }
                })).Wait();

                return true;
            }
            catch (Exception e)
            {
                return e;
            }
        }
    }
}
