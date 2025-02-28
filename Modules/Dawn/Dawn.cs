using AccountGen.Services;
using AccountGen.Utils;
using Newtonsoft.Json.Linq;
using RandomNameGeneratorLibrary;
using System.Collections.Generic;

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
        private readonly string dawnTurnstileKey = "0x4AAAAAAA48wVDquA-98fyV";

        internal void GenerateAccounts(int quantity)
        {
            if (string.IsNullOrWhiteSpace(capsolverKey))
            {
                LoggingHelper.Log("Capsolver key is empty! Cannot continue", LoggingHelper.LogType.Error);
                return;
            }

            caphandler.SetApiKey(capsolverKey);

            // Get Proxies
            var proxies = ProxyHelper.GetProxies();
            if (proxies.Count == 0)
            {
                LoggingHelper.Log("Did not find any proxies! Cannot continue", LoggingHelper.LogType.Error);
                return;
            }

            var referralCodes = SettingsHelper.GetDawnReferralCodes();
            if (referralCodes.Count == 0)
            {
                LoggingHelper.Log("Did not find any referral codes!", LoggingHelper.LogType.Error);
            }

            LoggingHelper.Log($"Starting to generate {quantity} accounts");
            List<string> accounts = [];
            accounts.Add("email,password,proxy");
            var delay = SettingsHelper.GetGenDelay();
            for (int i = 0; i < quantity; i++)
            {
                var account = GenerateAccount(proxies[i % proxies.Count], referralCodes.Count > 0 ? referralCodes[i % referralCodes.Count] : "");
                if (account != ",,")
                {
                    accounts.Add(account);
                }
                else
                {
                    i--;
                }
                LoggingHelper.Log($"Waiting {delay}ms before continuing");
                Thread.Sleep(delay);
            }

            LoggingHelper.Log("Finished generating accounts!", LoggingHelper.LogType.Success);

            var filePath = $"{SettingsHelper.GetOutputFolder()}dawnAccounts-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.csv";
            File.WriteAllLines(filePath, accounts);

            LoggingHelper.Log($"Successfully saved accounts to {filePath}", LoggingHelper.LogType.Success);
            LoggingHelper.Log("Waiting 10s before starting email verifications");

            Thread.Sleep(10000);

            LoggingHelper.Log("Starting to verify accounts");

            for (int i = 1; i < accounts.Count; ++i)
            {
                var account = accounts[i].Split(',');
                var username = account[0];
                var proxy = account[2];
                LoggingHelper.Log($"Starting verification for {username}");

                if (VerifyAccount(username, proxy))
                {
                    LoggingHelper.Log($"Account {username} Successfully verified", LoggingHelper.LogType.Success);
                } 
                else
                {
                    LoggingHelper.Log($"Failed to verify account {username}, please try manually", LoggingHelper.LogType.Error);
                }
            }

            LoggingHelper.Log("Task completed successfully", LoggingHelper.LogType.Success);
        }

        private string GenerateAccount(string proxy, string referralCode = "")
        {
            LoggingHelper.Log($"Starting to generate account with proxy: {proxy}");

            var tokenTask = caphandler.GetTurnstile("https://dashboard.dawninternet.com/signup", dawnTurnstileKey);
            tokenTask.Wait();
            var token = tokenTask.Result ?? "";

            string url = "https://ext-api.dawninternet.com/chromeapi/dawn/v2/dashboard/user/validate-register";

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
                { "referralCode", referralCode },
                { "token", token }
            };

            var res = tlsSession.Post(url, headers, JObject.FromObject(body).ToString(), differentSession: true, proxy: proxy);

            if (res == null)
            {
                LoggingHelper.Log("Error generating account (null)", LoggingHelper.LogType.Error);
                return ",,";
            }

            if (res.Status != 200)
            {
                LoggingHelper.Log($"Error generating account ({res.Status})", LoggingHelper.LogType.Error);
                return ",,";
            }

            try
            {
                var jsonBody = JObject.Parse(res.Body);
                if (jsonBody["success"].Value<bool?>() == false)
                {
                    LoggingHelper.Log("Failed to create account", LoggingHelper.LogType.Error);
                    return ",,";
                }

                LoggingHelper.Log("Generated new account", LoggingHelper.LogType.Success);

                return $"{email},{password},{ProxyHelper.UnformatProxy(proxy)}";

            } 
            catch (Exception ex) 
            {
                LoggingHelper.Log("Error checking if success", LoggingHelper.LogType.Error);
                LoggingHelper.Log(ex.ToString());
                return ",,";
            }
        }

        private bool VerifyAccount(string username, string proxy)
        {
            var verificationId = IMAPService.GetInstance().GetVerificationIdFromEmail(username);
            
            if (string.IsNullOrWhiteSpace(verificationId))
            {
                LoggingHelper.Log("Could not find verification id", LoggingHelper.LogType.Error);
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
                LoggingHelper.Log("Response was null", LoggingHelper.LogType.Error);
                return false;
            }

            if (res.Status != 200)
            {
                LoggingHelper.Log($"Status Code {res.Status}", LoggingHelper.LogType.Error);
                return false;
            }

            try
            {
                var jsonBody = JObject.Parse(res.Body);
                if (jsonBody["success"].Value<bool?>() == false)
                {
                    LoggingHelper.Log("Got the following response when verifying: " + jsonBody.ToString(), LoggingHelper.LogType.Error);
                    return false;
                }

                return true;

            }
            catch (Exception ex)
            {
                LoggingHelper.Log("Error checking if success", LoggingHelper.LogType.Error);
                LoggingHelper.Log(ex.ToString());
                return false;
            }
        }

    }
}
