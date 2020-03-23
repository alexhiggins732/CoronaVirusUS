namespace TwitCreds
{
    public interface ISettingsProvider
    {
        string GetValue(string key);
    }
}
