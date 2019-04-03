using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace Wit.Twitter
{
    public class APIException : Exception
    {
        public Objects.Errors Info { get; private set; }
        public HttpResponseMessage Response { get; private set; }

        public APIException(HttpResponseMessage res) : base("Twitter API Server returned an error.")
        {
            InitException(res).Wait();
        }

        private async Task InitException(HttpResponseMessage res)
        {
            try
            {
                Info = JsonConvert.DeserializeObject<Objects.Errors>(await res.Content.ReadAsStringAsync());
            }
            catch (Exception)
            {
                Info = null;
            }
            finally
            {
                Response = res;
            }
        }
    }
}
