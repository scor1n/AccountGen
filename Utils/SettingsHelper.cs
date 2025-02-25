using AccountGen.Classes;
using MailKit.Net.Imap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountGen.Utils
{
    internal static class SettingsHelper
    {
        private static Settings settings = new()
        {
            CapsolverKey = "",
            CatchallDomain = "",
            TwoLetterCountryCode = "DE",
            DawnReferralCode = "",
            GrassReferralCode = "",
            InputProxyFile = "./Input/Proxies.txt",
            OutputFolder = "./Output/",
            ImapHost = "imap.gmail.com",
            ImapPort = "993",
            ImapUsername = "",
            ImapPassword = "",
            GenDelay = 0
        };

        private static readonly string settingsFilename = "settings.json";

        public static void InitSettings()
        {
            if (File.Exists(settingsFilename))
            {
                using StreamReader r = new(settingsFilename);
                string json = r.ReadToEnd();
                settings = JObject.Parse(json).ToObject<Settings>() ?? settings;
            } 
            else
            {
                Console.WriteLine("Settings.json does not exist. Creating a new one which you must fill in with your information");
                var jsonString = JObject.FromObject(settings).ToString(Formatting.Indented);
                File.WriteAllText(settingsFilename, jsonString);
            }
        }

        public static string GetCapsolverKey()
        {
            if (File.Exists(settingsFilename))
            {
                using StreamReader r = new(settingsFilename);
                string json = r.ReadToEnd();
                settings = JObject.Parse(json).ToObject<Settings>() ?? settings;
            }
            else
            {
                Console.WriteLine("Settings.json does not exist. Creating a new one which you must fill in with your information");
                var jsonString = JObject.FromObject(settings).ToString(Formatting.Indented);
                File.WriteAllText(settingsFilename, jsonString);
                return "";
            }

            return settings.CapsolverKey;
        }

        public static string GetCatchallDomain()
        {
            if (File.Exists(settingsFilename))
            {
                using StreamReader r = new(settingsFilename);
                string json = r.ReadToEnd();
                settings = JObject.Parse(json).ToObject<Settings>() ?? settings;
            }
            else
            {
                Console.WriteLine("Settings.json does not exist. Creating a new one which you must fill in with your information");
                var jsonString = JObject.FromObject(settings).ToString(Formatting.Indented);
                File.WriteAllText(settingsFilename, jsonString);
                return "";
            }

            return settings.CatchallDomain;
        }

        public static string GetTwoLetterCountryCode()
        {
            if (File.Exists(settingsFilename))
            {
                using StreamReader r = new(settingsFilename);
                string json = r.ReadToEnd();
                settings = JObject.Parse(json).ToObject<Settings>() ?? settings;
            }
            else
            {
                Console.WriteLine("Settings.json does not exist. Creating a new one which you must fill in with your information");
                var jsonString = JObject.FromObject(settings).ToString(Formatting.Indented);
                File.WriteAllText(settingsFilename, jsonString);
                return "DE";
            }

            return settings.TwoLetterCountryCode;
        }

        public static string GetDawnReferralCode()
        {
            if (File.Exists(settingsFilename))
            {
                using StreamReader r = new(settingsFilename);
                string json = r.ReadToEnd();
                settings = JObject.Parse(json).ToObject<Settings>() ?? settings;
            }
            else
            {
                Console.WriteLine("Settings.json does not exist. Creating a new one which you must fill in with your information");
                var jsonString = JObject.FromObject(settings).ToString(Formatting.Indented);
                File.WriteAllText(settingsFilename, jsonString);
                return "";
            }

            return settings.DawnReferralCode;
        }

        public static string GetGrassReferralCode()
        {
            if (File.Exists(settingsFilename))
            {
                using StreamReader r = new(settingsFilename);
                string json = r.ReadToEnd();
                settings = JObject.Parse(json).ToObject<Settings>() ?? settings;
            }
            else
            {
                Console.WriteLine("Settings.json does not exist. Creating a new one which you must fill in with your information");
                var jsonString = JObject.FromObject(settings).ToString(Formatting.Indented);
                File.WriteAllText(settingsFilename, jsonString);
                return "EV08HjfB81lPdgM";
            }

            return settings.GrassReferralCode;
        }

        public static string GetInputProxyFile()
        {
            if (File.Exists(settingsFilename))
            {
                using StreamReader r = new(settingsFilename);
                string json = r.ReadToEnd();
                settings = JObject.Parse(json).ToObject<Settings>() ?? settings;
            }
            else
            {
                Console.WriteLine("Settings.json does not exist. Creating a new one which you must fill in with your information");
                var jsonString = JObject.FromObject(settings).ToString(Formatting.Indented);
                File.WriteAllText(settingsFilename, jsonString);
                return "./Input/Proxies.txt";
            }

            return settings.InputProxyFile;
        }

        public static string GetOutputFolder()
        {
            if (File.Exists(settingsFilename))
            {
                using StreamReader r = new(settingsFilename);
                string json = r.ReadToEnd();
                settings = JObject.Parse(json).ToObject<Settings>() ?? settings;
            }
            else
            {
                Console.WriteLine("Settings.json does not exist. Creating a new one which you must fill in with your information");
                var jsonString = JObject.FromObject(settings).ToString(Formatting.Indented);
                File.WriteAllText(settingsFilename, jsonString);
                return "./Output/";
            }

            return settings.OutputFolder;
        }

        public static string GetImapHost()
        {
            if (File.Exists(settingsFilename))
            {
                using StreamReader r = new(settingsFilename);
                string json = r.ReadToEnd();
                settings = JObject.Parse(json).ToObject<Settings>() ?? settings;
            }
            else
            {
                Console.WriteLine("Settings.json does not exist. Creating a new one which you must fill in with your information");
                var jsonString = JObject.FromObject(settings).ToString(Formatting.Indented);
                File.WriteAllText(settingsFilename, jsonString);
                return "imap.gmail.com";
            }

            return settings.ImapHost;
        }

        public static string GetImapPort()
        {
            if (File.Exists(settingsFilename))
            {
                using StreamReader r = new(settingsFilename);
                string json = r.ReadToEnd();
                settings = JObject.Parse(json).ToObject<Settings>() ?? settings;
            }
            else
            {
                Console.WriteLine("Settings.json does not exist. Creating a new one which you must fill in with your information");
                var jsonString = JObject.FromObject(settings).ToString(Formatting.Indented);
                File.WriteAllText(settingsFilename, jsonString);
                return "993";
            }

            return settings.ImapPort;
        }

        public static string GetImapUsername()
        {
            if (File.Exists(settingsFilename))
            {
                using StreamReader r = new(settingsFilename);
                string json = r.ReadToEnd();
                settings = JObject.Parse(json).ToObject<Settings>() ?? settings;
            }
            else
            {
                Console.WriteLine("Settings.json does not exist. Creating a new one which you must fill in with your information");
                var jsonString = JObject.FromObject(settings).ToString(Formatting.Indented);
                File.WriteAllText(settingsFilename, jsonString);
                return "";
            }

            return settings.ImapUsername;
        }

        public static string GetImapPassword()
        {
            if (File.Exists(settingsFilename))
            {
                using StreamReader r = new(settingsFilename);
                string json = r.ReadToEnd();
                settings = JObject.Parse(json).ToObject<Settings>() ?? settings;
            }
            else
            {
                Console.WriteLine("Settings.json does not exist. Creating a new one which you must fill in with your information");
                var jsonString = JObject.FromObject(settings).ToString(Formatting.Indented);
                File.WriteAllText(settingsFilename, jsonString);
                return "";
            }

            return settings.ImapPassword;
        }

        public static int GetGenDelay()
        {
            if (File.Exists(settingsFilename))
            {
                using StreamReader r = new(settingsFilename);
                string json = r.ReadToEnd();
                settings = JObject.Parse(json).ToObject<Settings>() ?? settings;
            }
            else
            {
                Console.WriteLine("Settings.json does not exist. Creating a new one which you must fill in with your information");
                var jsonString = JObject.FromObject(settings).ToString(Formatting.Indented);
                File.WriteAllText(settingsFilename, jsonString);
                return 0;
            }

            return settings.GenDelay;
        }
    }
}
