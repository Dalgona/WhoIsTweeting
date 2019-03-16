namespace Wit.UI.Core
{
    public interface IStringProvider
    {
        string GetString(string key, string defaultString = null);
    }
}
