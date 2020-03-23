namespace TwitCreds
{
    public class SettingsProvider : ISettingsProvider
    {
        public static SettingsProvider Default = new SettingsProvider(new ApiSecretsKeyValueStore());
        private KeyValueStore keyValueStore;
        public SettingsProvider(KeyValueStore store)
        {
            this.keyValueStore = store;
        }

        public string GetValue(string key) => keyValueStore.GetKey(key);

    }
}
