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
        private readonly API _api = new API();
        private HashSet<string> _userIds;

        #region Properties

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

        #endregion

        public async Task<TwitterApiResult<bool>> SetAccessToken(Func<string, string> getVerifier)
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

        public async Task<TwitterApiResult<UserListItem>> CheckUser()
        {
            try
            {
                User user = await _api.Get<User>("/1.1/account/verify_credentials.json");

                CursoredIdStrings ids =
                    await _api.Get<CursoredIdStrings>(
                        "/1.1/friends/ids.json",
                        new NameValueCollection
                        {
                            { "user_id", user.IdStr },
                            { "stringify_id", "true" },
                            { "count", "5000" }
                        }
                    );

                _userIds = new HashSet<string>(ids.Ids);

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

        public async Task<TwitterApiResult<IEnumerable<UserListItem>>> RetrieveFollowings()
        {
            UserListItem.LastUpdated = DateTime.Now;
            HashSet<string> userIdsCopy = new HashSet<string>(_userIds ?? Enumerable.Empty<string>());
            List<UserListItem> list = new List<UserListItem>();

            try
            {
                do
                {
                    HashSet<string> batch = new HashSet<string>(userIdsCopy.Take(100));
                    string data = string.Join(",", batch);

                    List<User> tmp =
                        await _api.Post<List<User>>("/1.1/users/lookup.json", null, new NameValueCollection
                        {
                            { "user_id", data },
                            { "include_entities", "true" }
                        });

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

        public async Task<TwitterApiResult<bool>> PostTweet(string content)
        {
            try
            {
                await _api.Post("/1.1/statuses/update.json", null, new NameValueCollection
                {
                    { "status", content }
                });

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
