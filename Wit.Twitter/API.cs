using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Wit.Twitter
{
    public class API
    {
        private static readonly string _apiRoot = "https://api.twitter.com";

        private static readonly IContractResolver _contractResolver = new DefaultContractResolver()
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        };

        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            DateFormatString = "ddd MMM dd HH:mm:ss +0000 yyyy",
            ContractResolver = _contractResolver
        };

        private HttpClient _client;

        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string Token { get; set; }
        public string TokenSecret { get; set; }
        public string OAuthCallback { get; set; }

        public int HttpTimeout
        {
            get => (int)_client.Timeout.TotalSeconds;
            set => _client = new HttpClient { Timeout = TimeSpan.FromSeconds(value) };
        }

        // Constructor
        public API(string consumerKey, string consumerSecret)
        {
            _client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
            Token = "";
            TokenSecret = "";
            OAuthCallback = "";
        }

        // Send OAuth signed requests.
        private async Task<HttpResponseMessage> SendRequest(
            HttpMethod method,
            string resource,
            NameValueCollection query = null,
            NameValueCollection data = null)
        {
            string baseUrl = _apiRoot + resource;    // api url without query string
            string requestUrl = baseUrl;    // api url with query string (if any)
            string queryString = query != null ? PercentEncode(query) : "";
            if (!queryString.Equals("")) requestUrl += "?" + queryString;

            string headerString = GenerateAuthHeader(method, baseUrl, query, data);

            HttpRequestMessage request = new HttpRequestMessage(method, requestUrl);
            request.Headers.Add("Authorization", headerString);
            if (method == HttpMethod.Post && data != null)
                request.Content = new FormUrlEncodedContent(
                    from k in data.AllKeys from v in data.GetValues(k) select new KeyValuePair<string, string>(k, v));

            HttpResponseMessage res = await _client.SendAsync(request);

            if (res.StatusCode != System.Net.HttpStatusCode.OK)
                throw new APIException(res);

            return res;
        }

        public async Task<HttpResponseMessage> Get(
            string resource,
            NameValueCollection query = null)
            => await SendRequest(HttpMethod.Get, resource, query);

        public async Task<T> Get<T>(
            string resource,
            NameValueCollection query = null)
            => JsonConvert.DeserializeObject<T>(
                await (await SendRequest(HttpMethod.Get, resource, query)).Content.ReadAsStringAsync(), _jsonSettings);

        public async Task<HttpResponseMessage> Post(
            string resource,
            NameValueCollection query = null,
            NameValueCollection data = null)
            => await SendRequest(HttpMethod.Post, resource, query, data);

        public async Task<T> Post<T>(
            string resource,
            NameValueCollection query = null,
            NameValueCollection data = null)
            => JsonConvert.DeserializeObject<T>(
                await (await SendRequest(HttpMethod.Post, resource, query, data)).Content.ReadAsStringAsync(), _jsonSettings);

        public async Task RequestToken(Func<string, string> callback)
        {
            var res = await Post("/oauth/request_token");
            var tokens = from i in (await res.Content.ReadAsStringAsync()).Split('&')
                         select i.Split('=');
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var i in tokens) dict.Add(i[0], i[1]);
            Token = dict["oauth_token"];
            TokenSecret = dict["oauth_token_secret"];

            var pin = callback(_apiRoot + $"/oauth/authenticate?oauth_token={Token}");

            res = await Post("/oauth/access_token", null, new NameValueCollection { { "oauth_verifier", pin } });
            tokens = from i in (await res.Content.ReadAsStringAsync()).Split('&')
                        select i.Split('=');
            dict = new Dictionary<string, string>();
            foreach (var i in tokens) dict.Add(i[0], i[1]);
            Token = dict["oauth_token"];
            TokenSecret = dict["oauth_token_secret"];
        }

        public async Task<string> GetAuthenticateUrl()
        {
            var res = await Post("/oauth/request_token");
            var tokens = from i in (await res.Content.ReadAsStringAsync()).Split('&')
                         select i.Split('=');
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var i in tokens) dict.Add(i[0], i[1]);
            Token = dict["oauth_token"];
            TokenSecret = dict["oauth_token_secret"];
            return _apiRoot + $"/oauth/authenticate?oauth_token={Token}";
        }

        public async Task GetTokenFromPin(string pin)
        {
            var res = await Post("/oauth/access_token", null, new NameValueCollection { { "oauth_verifier", pin } });
            var tokens = from i in (await res.Content.ReadAsStringAsync()).Split('&')
                         select i.Split('=');
            var dict = new Dictionary<string, string>();
            foreach (var i in tokens) dict.Add(i[0], i[1]);
            Token = dict["oauth_token"];
            TokenSecret = dict["oauth_token_secret"];
        }

        #region OAuth Helper Functions

        private static string PercentEncode(NameValueCollection nvc)
        {
            var values = (from k in nvc.AllKeys orderby k ascending
                         from v in nvc.GetValues(k)
                         select $"{Uri.EscapeDataString(k)}={Uri.EscapeDataString(v)}");
            return string.Join("&", values);
        }

        private static string GenerateNonce()
        {
            Random rand = new Random();
            byte[] x = new byte[32];
            rand.NextBytes(x);
            return Regex.Replace(Convert.ToBase64String(x), "[+/=]", "");
        }

        private static string GenerateTimestamp()
            => ((long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds)).ToString();

        private string GenerateAuthHeader(
            HttpMethod method,
            string baseUrl,
            NameValueCollection query = null,
            NameValueCollection data = null)
        {
            NameValueCollection parameters = new NameValueCollection
            {
                { "oauth_consumer_key", ConsumerKey },
                { "oauth_nonce", GenerateNonce() },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", GenerateTimestamp() },
                { "oauth_version", "1.0" },
            };
            if (!Token.Equals(""))
                parameters.Add("oauth_token", Token);
            else if (baseUrl.Contains("request_token"))
                parameters.Add("oauth_callback", OAuthCallback);

            NameValueCollection headerParams = new NameValueCollection(parameters);
            if (query != null) parameters.Add(query);
            if (method == HttpMethod.Post && data != null) parameters.Add(data);
            string paramString = PercentEncode(parameters);

            string signBase = method.ToString() + "&" + Uri.EscapeDataString(baseUrl) + "&" + Uri.EscapeDataString(paramString);
            string signKey = Uri.EscapeDataString(ConsumerSecret) + "&" + Uri.EscapeDataString(TokenSecret);

            HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(signKey));
            string signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(signBase)));
            headerParams.Add("oauth_signature", signature);
            string headerString = "OAuth "
                + string.Join(", ", (from k in headerParams.AllKeys
                                     orderby k ascending
                                     from v in headerParams.GetValues(k)
                                     select $"{Uri.EscapeDataString(k)}=\"{Uri.EscapeDataString(v)}\""));

            return headerString;
        }

        #endregion
    }
}
