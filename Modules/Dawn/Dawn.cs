using AccountGen.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AccountGen.Modules.Dawn
{
    internal class Dawn
    {
        private CapsolverHandler caphandler = new CapsolverHandler();
        private TLSSession tlsSession = new();
        private Random random = new Random();
        private readonly string catchallDomain = SettingsHelper.GetCatchallDomain();
        private readonly string capsolverKey = SettingsHelper.GetCapsolverKey();
        private readonly string twoLetterCountryCode = SettingsHelper.GetTwoLetterCountryCode();
        private readonly string dawnReferralCode = SettingsHelper.GetDawnReferralCode();

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

            var filePath = $"{SettingsHelper.GetOutputFolder()}dawnAccounts-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.csv";
            File.WriteAllLines(filePath, accounts);

            Console.WriteLine($"Successfully saved accounts to {filePath}");
        }

        private string GenerateAccount(string proxy)
        {
            Console.WriteLine($"Starting to generate account with proxy: {proxy}");
            string key = "0x4AAAAAAA48wVDquA-98fyV";

            var tokenTask = caphandler.GetTurnstile("https://dashboard.dawninternet.com/signup", key);
            tokenTask.Wait();
            var token = tokenTask.Result ?? "";

            string url = "https://ext-api.dawninternet.com/chromeapi/dawn/v2/dashboard/user/validate-register?appid=67afb7b052e901649cb1b685";

            var headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            };

            var names = GenerateFirstAndLastName();
            var email = $"{names[0]}.{names[1]}{random.Next(99)}@{catchallDomain}";
            var password = PasswordGenerator.GeneratePassword();

            var body = new Dictionary<string, object>
            {
                { "browserName", "chrome" },
                { "country", twoLetterCountryCode.ToUpper() },
                { "email", email },
                { "firstname", names[0] },
                { "isMarketing", false },
                { "lastname", names[1] },
                { "mobile", "" },
                { "password", password },
                { "referralCode", dawnReferralCode },
                { "token", token }
            };

            var res = tlsSession.Post(url, headers, JObject.FromObject(body).ToString(), differentSession: true, proxy: proxy);

            if (res == null)
            {
                Console.WriteLine("Error generating account (null)");
                return ",,";
            }

            if (res.Status != 200)
            {
                Console.WriteLine($"Error generating account ({res.Status})");
                return ",,";
            }

            try
            {
                var jsonBody = JObject.Parse(res.Body);
                if (jsonBody["success"].Value<bool?>() == false)
                {
                    Console.WriteLine("Failed to create account");
                    return ",,";
                }

                Console.WriteLine("Generated new account");

                return $"{email},{password},{ProxyHelper.UnformatProxy(proxy)}";

            } 
            catch (Exception ex) 
            {
                Console.WriteLine("Error checking if success");
                Console.WriteLine(ex.ToString());
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
