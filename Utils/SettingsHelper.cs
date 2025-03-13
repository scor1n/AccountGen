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
            DawnReferralCodesFile = "./Input/DawnReferralCodes.txt",
            GrassReferralCodesFile = "./Input/GrassReferralCodes.txt",
            InputProxyFile = "./Input/Proxies.txt",
            InputLoggedInAccountsFile = "./Input/LoggedInAccounts.csv",
            InputEmails = "./Input/Emails.txt",
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

        public static List<string> GetDawnReferralCodes()
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
                return [];
            }

            List<string> formatedList = [];

            try
            {
                formatedList = File.ReadAllLines(settings.DawnReferralCodesFile)
                .Select(line => line.Trim())
                .ToList();
            }
            catch (Exception  ex)
            {
                LoggingHelper.Log(ex.Message, LoggingHelper.LogType.Error);
            }

            return formatedList;
        }

        public static List<string> GetGrassReferralCodes()
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
                return [];
            }

            List<string> formatedList = [];

            try
            {
                formatedList = File.ReadAllLines(settings.GrassReferralCodesFile)
                .Select(line => line.Trim())
                .ToList();
            }
            catch (Exception ex)
            {
                LoggingHelper.Log(ex.Message, LoggingHelper.LogType.Error);
            }

            return formatedList;
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

        public static string GetInputLoggedInAccountsFile()
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
                return "./Input/LoggedInAccounts.csv";
            }

            return settings.InputLoggedInAccountsFile;
        }

        public static List<string> GetEmails()
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
                return [];
            }

            List<string> formatedList = [];

            try
            {
                formatedList = File.ReadAllLines(settings.InputEmails ?? "")
                .Select(line => line.Trim())
                .ToList();
            }
            catch (Exception ex)
            {
                LoggingHelper.Log(ex.Message, LoggingHelper.LogType.Error);
            }

            return formatedList;
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
