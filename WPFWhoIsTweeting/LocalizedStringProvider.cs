using Wit.UI.Core;

namespace WhoIsTweeting
{
    public class LocalizedStringProvider : IStringProvider
    {
        public string GetString(string key, string defaultString = null)
        {
            return (string)(typeof(Strings).GetProperty(key)?.GetValue(null) ?? defaultString);
        }
    }
}
