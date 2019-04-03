using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Wit.Twitter;
using Wit.Twitter.Objects;

namespace Wit.Core
{
    class TwitterAdapter : ITwitterAdapter
    {
        API _api = new API("", "")
        {
            HttpTimeout = 10,
            OAuthCallback = "oob"
        };

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

        public int HttpTimeout
        {
            get => _api.HttpTimeout;
            set => _api.HttpTimeout = value;
        }

        public async Task<TwitterApiResult<bool>> SetAccessTokenAsync(Func<string, string> getVerifier)
        {
            try
            {
                AccessToken = AccessTokenSecret = "";

                await _api.RequestToken(getVerifier);

                return true;
            }
            catch (AggregateException e)
            {
                return e.InnerException;
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public TwitterApiResult<UserListItem> CheckUser()
        {
            try
            {
                User user = Task.Run(() => _api.Get<User>("/1.1/account/verify_credentials.json")).Result;

                return new UserListItem(user.IdStr, user.Name, user.ScreenName, user.Status);
            }
            catch (AggregateException e)
            {
                return e.InnerException;
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

                return ids.Ids;
            }
            catch (AggregateException e)
            {
                return e.InnerException;
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

                    list.AddRange(from u in tmp select new UserListItem(u.IdStr, u.Name, u.ScreenName, u.Status));
                    userIdsCopy.ExceptWith(batch);
                } while (userIdsCopy.Count != 0);

                return list;
            }
            catch (AggregateException e)
            {
                return e.InnerException;
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
            catch (AggregateException e)
            {
                return e.InnerException;
            }
            catch (Exception e)
            {
                return e;
            }
        }
    }
}
