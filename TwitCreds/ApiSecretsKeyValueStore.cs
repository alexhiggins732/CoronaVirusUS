using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitCreds
{
    public class ApiSecretsKeyValueStore : KeyValueStore
    {
        Dictionary<string, string> secrets;
        public ApiSecretsKeyValueStore()
        {
            var settingsFile = GetSettingsFile();

            string settingsJson = string.Empty;
            if (!settingsFile.Exists)
            {
                settingsFile.Directory.Create();
                settingsJson = GetDefaultJson();
                File.WriteAllText(settingsFile.FullName, settingsJson);
                Prompt(settingsFile);

            }
            settingsJson = File.ReadAllText(settingsFile.FullName);

            secrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(settingsJson);
            while (secrets.ToList().Any(x => string.IsNullOrEmpty(x.Value)))
            {
                Prompt(settingsFile);
            }
        }
        private FileInfo GetSettingsFile()
        {
            var current = Process.GetCurrentProcess().MainModule.FileName;
            var dir = new DirectoryInfo(current);
            var root = dir.Root;
            var settingsDirectoy = new DirectoryInfo(Path.Combine(root.FullName, "invisettings"));
            var settingsFile = new FileInfo(Path.Combine(settingsDirectoy.FullName, "settings.json"));
            return settingsFile;
        }

        private void Prompt(FileInfo settingsFile)
        {
            settingsFile.Delete();
            File.WriteAllText(settingsFile.FullName, GetDefaultJson());
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine($"Settings file must be completed at '{settingsFile.FullName}'");
            Console.WriteLine("Program will continue when notepad has been closed.");
            var p = Process.Start("notepad", settingsFile.FullName);
            p.WaitForExit();

        }

        private string GetDefaultJson()
        {
            var d = new Dictionary<string, string>
            {
                { TwitCreds.SettingsConstants.ACCESS_TOKEN, "" },
                { TwitCreds.SettingsConstants.ACCESS_TOKEN_SECRET, "" },
                { TwitCreds.SettingsConstants.CONSUMER_KEY, "" },
                { TwitCreds.SettingsConstants.CONSUMER_SECRET, "" },
            };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(d, Formatting.Indented);
            return json;
        }

        public override string GetKey(string key) => secrets[key];

    }
}
