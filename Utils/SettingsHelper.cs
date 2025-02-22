using AccountGen.Classes;
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
            GrassReferralCode = "EV08HjfB81lPdgM",
            InputProxyFile = "./Input/Proxies.txt",
            OutputFolder = "./Output/"
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
    }
}
