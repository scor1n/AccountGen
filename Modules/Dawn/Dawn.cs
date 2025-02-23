using AccountGen.Services;
using AccountGen.Utils;
using Newtonsoft.Json.Linq;
using RandomNameGeneratorLibrary;

namespace AccountGen.Modules.Dawn
{
    internal class Dawn
    {
        private CapsolverHandler caphandler = new CapsolverHandler();
        private TLSSession tlsSession = new();
        private Random random = new Random();
        private PersonNameGenerator nameGenerator = new PersonNameGenerator();
        private readonly string catchallDomain = SettingsHelper.GetCatchallDomain();
        private readonly string capsolverKey = SettingsHelper.GetCapsolverKey();
        private readonly string twoLetterCountryCode = SettingsHelper.GetTwoLetterCountryCode();
        private readonly string dawnReferralCode = SettingsHelper.GetDawnReferralCode();
        private readonly string dawnTurnstileKey = "0x4AAAAAAA48wVDquA-98fyV";

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
                var account = GenerateAccount(proxies[random.Next(proxies.Count)]);
                if (account != ",,")
                {
                    accounts.Add(account);
                }
                else
                {
                    i--;
                }
            }

            Console.WriteLine("Finished generating accounts!");

            var filePath = $"{SettingsHelper.GetOutputFolder()}dawnAccounts-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.csv";
            File.WriteAllLines(filePath, accounts);

            Console.WriteLine($"Successfully saved accounts to {filePath}");
            Console.WriteLine("Waiting 10s before starting email verifications");

            Thread.Sleep(10000);

            Console.WriteLine("Starting to verify accounts");

            for (int i = 1; i < accounts.Count; ++i)
            {
                var account = accounts[i].Split(',');
                var username = account[0];
                var proxy = account[2];
                Console.WriteLine($"Starting verification for {username}");

                if (VerifyAccount(username, proxy))
                {
                    Console.WriteLine($"Account {username} Successfully verified");
                } 
                else
                {
                    Console.WriteLine($"Failed to verify account {username}, please try manually");
                }
            }

            Console.WriteLine("Task completed successfully");
        }

        private string GenerateAccount(string proxy)
        {
            Console.WriteLine($"Starting to generate account with proxy: {proxy}");

            var tokenTask = caphandler.GetTurnstile("https://dashboard.dawninternet.com/signup", dawnTurnstileKey);
            tokenTask.Wait();
            var token = tokenTask.Result ?? "";

            string url = "https://ext-api.dawninternet.com/chromeapi/dawn/v2/dashboard/user/validate-register?appid=67afb7b052e901649cb1b685";

            var headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            };

            var names = nameGenerator.GenerateRandomFirstAndLastName().Split(' ');
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

        private bool VerifyAccount(string username, string proxy)
        {
            var verificationId = IMAPService.GetInstance().GetVerificationIdFromEmail(username);
            
            if (string.IsNullOrWhiteSpace(verificationId))
            {
                Console.WriteLine("Could not find verification id");
                return false;
            }

            var tokenTask = caphandler.GetTurnstile("https://verify.dawninternet.com/chromeapi/dawn/v1/userverify/verifyconfirm", dawnTurnstileKey);
            tokenTask.Wait();
            var token = tokenTask.Result ?? "";

            string url = $"https://verify.dawninternet.com/chromeapi/dawn/v1/userverify/verifycheck?key={verificationId}";

            var headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            };

            var body = new Dictionary<string, object>
            {
                { "token", token }
            };

            var res = tlsSession.Post(url, headers, JObject.FromObject(body).ToString(), differentSession: true, proxy: ProxyHelper.FormatProxy(proxy));

            if (res == null)
            {
                Console.WriteLine("Response was null");
                return false;
            }

            if (res.Status != 200)
            {
                Console.WriteLine($"Status Code {res.Status}");
                return false;
            }

            try
            {
                var jsonBody = JObject.Parse(res.Body);
                if (jsonBody["success"].Value<bool?>() == false)
                {
                    Console.WriteLine("Got the following response when verifying: " + jsonBody.ToString());
                    return false;
                }

                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking if success");
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

    }
}
