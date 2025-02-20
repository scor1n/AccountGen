using DawnAccountGen.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DawnAccountGen.Modules.Grass
{
    internal class Grass
    {
        private CapsolverHandler caphandler = new CapsolverHandler();
        private TLSSession tlsSession = new();
        private Random random = new Random();
        private readonly string catchallDomain = SettingsHelper.GetCatchallDomain();
        private readonly string capsolverKey = SettingsHelper.GetCapsolverKey();
        private readonly string grassReferralCode = SettingsHelper.GetGrassReferralCode();

        internal void GenerateAccounts(int quantity)
        {
            if (string.IsNullOrWhiteSpace(capsolverKey))
            {
                Console.WriteLine("Capsolver key is empty! Cannot continue");
                return;
            }

            caphandler.SetApiKey(capsolverKey);

            // Get Proxies
            var proxies = ProxyHelper.GetProxies();
            if (proxies.Count == 0)
            {
                Console.WriteLine("Did not find any proxies! Cannot continue");
                return;
            }

            Console.WriteLine($"Starting to generate {quantity} accounts");
            List<string> accounts = new List<string>();
            accounts.Add("email,password,proxy");
            for (int i = 0; i < quantity; i++)
            {
                accounts.Add(GenerateAccount(proxies[quantity % proxies.Count]));
            }

            Console.WriteLine("Finished generating accounts!");

            var filePath = $"{SettingsHelper.GetOutputFolder()}grassAccounts-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.csv";
            File.WriteAllLines(filePath, accounts);

            Console.WriteLine($"Successfully saved accounts to {filePath}");
        }

        private string GenerateAccount(string proxy)
        {
            Console.WriteLine($"Starting to generate account with proxy: {proxy}");
            string key = "6LeeT-0pAAAAAFJ5JnCpNcbYCBcAerNHlkK4nm6y";

            var tokenTask = caphandler.GetRecaptchaV2Enterprise("https://app.getgrass.io/register", key);
            tokenTask.Wait();
            var token = tokenTask.Result ?? "";

            string url = "https://api.getgrass.io/register";

            var headers = new Dictionary<string, string>
            {
                { "sec-ch-ua-platform", "\"Windows\"" },
                { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/132.0.0.0 Safari/537.36" },
                { "Accept", "application/json, text/plain, */*" },
                { "sec-ch-ua", "\"Not A(Brand\";v=\"8\", \"Chromium\";v=\"132\", \"Google Chrome\";v=\"132\"" },
                { "Content-Type", "application/json" },
                { "sec-ch-ua-mobile", "?0" },
                { "Origin", "https://app.getgrass.io" },
                { "Sec-Fetch-Site", "same-site" },
                { "Sec-Fetch-Mode", "cors" },
                { "Sec-Fetch-Dest", "empty" },
                { "Referer", "https://app.getgrass.io/register" },
                { "Accept-Encoding", "gzip, deflate, br, zstd" },
                { "Accept-Language", "en-US,en;q=0.9" }
            };

            var names = GenerateFirstAndLastName();
            var email = $"{names[0]}.{names[1]}{random.Next(99)}@{catchallDomain}";
            var password = PasswordGenerator.GeneratePassword();

            var body = new Dictionary<string, object>
            {
                { "email", email },
                { "password", password },
                { "role", "USER" },
                { "referralCode", grassReferralCode },
                { "marketingEmailConsent", false },
                { "recaptchaToken", token }
            };

            var res = tlsSession.Post(url, headers, JObject.FromObject(body).ToString(), differentSession: true, proxy: proxy);

            if (res.Status == 200)
            {
                Console.WriteLine("Generated new account");

                return $"{email},{password},{ProxyHelper.UnformatProxy(proxy)}";
            }
            else
            {
                Console.WriteLine($"Error generating account ({res.Status})");
                return ",,";
            }
        }

        private static readonly string[] FirstNames = { "Alice", "Bob", "Charlie", "Diana", "Ethan", "Fiona", "George", "Hannah" };
        private static readonly string[] LastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis" };

        private static string[] GenerateFirstAndLastName()
        {
            Random random = new Random();
            string firstName = FirstNames[random.Next(FirstNames.Length)];
            string lastName = LastNames[random.Next(LastNames.Length)];
            return new string[] { firstName, lastName };
        }
    }
}
