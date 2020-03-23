namespace TwitCreds
{
    public class Settings
    {
        public static readonly string CONSUMER_KEY = SettingsProvider.Default.GetValue(nameof(CONSUMER_KEY));
        public static readonly string CONSUMER_SECRET = SettingsProvider.Default.GetValue(nameof(CONSUMER_SECRET));
        public static readonly string ACCESS_TOKEN = SettingsProvider.Default.GetValue(nameof(ACCESS_TOKEN));
        public static readonly string ACCESS_TOKEN_SECRET = SettingsProvider.Default.GetValue(nameof(ACCESS_TOKEN_SECRET));
    }
}
