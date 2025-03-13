using AccountGen.Services;
using AccountGen.Utils;
using Newtonsoft.Json.Linq;
using RandomNameGeneratorLibrary;

namespace AccountGen.Modules.Grass
{
    internal class Grass
    {
        private CapsolverHandler caphandler = new CapsolverHandler();
        private TLSSession tlsSession = new();
        private Random random = new Random();
        private PersonNameGenerator nameGenerator = new PersonNameGenerator();
        private readonly string catchallDomain = SettingsHelper.GetCatchallDomain();
        private readonly string capsolverKey = SettingsHelper.GetCapsolverKey();

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

            var referralCodes = SettingsHelper.GetGrassReferralCodes();
            if (referralCodes.Count == 0)
            {
                LoggingHelper.Log("Did not find any referral codes!", LoggingHelper.LogType.Error);
            }

            var emailAddresses = SettingsHelper.GetEmails();
            if (emailAddresses.Count == 0)
            {
                LoggingHelper.Log("No emails in email list, using catchall");
            }
            else if (emailAddresses.Count < quantity)
            {
                LoggingHelper.Log("Not enough emails provided, please add more or remove them to use catchall", LoggingHelper.LogType.Error);
                return;
            }

            LoggingHelper.Log($"Starting to generate {quantity} accounts");
            List<string> accounts = new List<string>();
            accounts.Add("email,password,proxy");
            var delay = SettingsHelper.GetGenDelay();
            for (int i = 0; i < quantity; i++)
            {
                var account = GenerateAccount(proxies[i % proxies.Count], referralCodes.Count > 0 ? referralCodes[i % referralCodes.Count] : "", emailAddresses.Count > 0 ? emailAddresses[i % emailAddresses.Count] : "");
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

            var filePath = $"{SettingsHelper.GetOutputFolder()}grassAccounts-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.csv";
            File.WriteAllLines(filePath, accounts);

            LoggingHelper.Log($"Successfully saved accounts to {filePath}", LoggingHelper.LogType.Success);

            LoggingHelper.Log("Waiting 10s before starting email verifications");

            Thread.Sleep(10000);

            LoggingHelper.Log("Starting to verify accounts");

            IMAPService.GetInstance().Connect();

            for (int i = 1; i < accounts.Count; ++i)
            {
                var account = accounts[i].Split(',');
                var username = account[0];
                var proxy = account[2];
                LoggingHelper.Log($"Starting verification for {username}");

                if (VerifyAccount(username, proxy))
                {
                    LoggingHelper.Log($"Account {username} Successfully verified", LoggingHelper.LogType.Success);
                    IMAPService.GetInstance().MarkLastEmailAsRead();
                }
                else
                {
                    LoggingHelper.Log($"Failed to verify account {username}, please try manually", LoggingHelper.LogType.Error);
                }
            }

            IMAPService.GetInstance().Disconnect();

            LoggingHelper.Log("Task completed successfully", LoggingHelper.LogType.Success);
        }

        private string GenerateAccount(string proxy, string referralCode = "", string email = "")
        {
            LoggingHelper.Log($"Starting to generate account with proxy: {proxy}");
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

            var names = nameGenerator.GenerateRandomFirstAndLastName().Split(' ');
            if (email.Length == 0)
            {
                email = $"{names[0]}.{names[1]}{random.Next(99)}@{catchallDomain}";
            }
            var password = PasswordGenerator.GeneratePassword();

            var body = new Dictionary<string, object>
            {
                { "email", email },
                { "password", password },
                { "role", "USER" },
                { "referralCode", referralCode },
                { "marketingEmailConsent", false },
                { "recaptchaToken", token }
            };

            var res = tlsSession.Post(url, headers, JObject.FromObject(body).ToString(), differentSession: true, proxy: proxy);

            if (res.Status == 200)
            {
                LoggingHelper.Log("Generated new account");

                return $"{email},{password},{ProxyHelper.UnformatProxy(proxy)}";
            }
            else
            {
                LoggingHelper.Log($"Error generating account ({res.Status})", LoggingHelper.LogType.Error);
                return ",,";
            }
        }

        private bool VerifyAccount(string username, string proxy)
        {
            var verificationLink = IMAPService.GetInstance().GetVerificationLinkFromEmailGrass(username);

            if (string.IsNullOrWhiteSpace(verificationLink))
            {
                LoggingHelper.Log("Could not find verification link", LoggingHelper.LogType.Error);
                return false;
            }

            var res = tlsSession.Get(verificationLink);
            if (res.Status != 302)
            {
                LoggingHelper.Log("Could not get redirect for grass verification", LoggingHelper.LogType.Error);
                return false;
            }

            var grassVerificationUrl = res.Headers["Location"].FirstOrDefault("");
            if (grassVerificationUrl.Length == 0)
            {
                LoggingHelper.Log("Could not get redirect link for grass verification", LoggingHelper.LogType.Error);
                return false;
            }

            var authenticationToken = grassVerificationUrl.Split("?token=")[1];
            if (string.IsNullOrWhiteSpace(authenticationToken))
            {
                LoggingHelper.Log("Could not get authentication token for grass verification", LoggingHelper.LogType.Error);
                return false;
            }

            string url = $"https://api.getgrass.io/confirmEmail";

            var headers = new Dictionary<string, string>
            {
                { "sec-ch-ua-platform", "\"Windows\"" },
                { "Authorization", authenticationToken },
                { "sec-ch-ua", "\"Chromium\";v=\"134\", \"Not:A-Brand\";v=\"24\", \"Google Chrome\";v=\"134\"" },
                { "sec-ch-ua-mobile", "?0" },
                { "Accept", "application/json, text/plain, */*" },
                { "DNT", "1" },
                { "Content-Type", "application/json" },
                { "Origin", "https://app.getgrass.io" },
                { "Sec-Fetch-Site", "same-site" },
                { "Sec-Fetch-Mode", "cors" },
                { "Sec-Fetch-Dest", "empty" },
                { "Referer", "https://app.getgrass.io/" },
                { "Accept-Encoding", "gzip, deflate, br, zstd" },
                { "Accept-Language", "en-US,en;q=0.9,fr;q=0.8" },
                { "Pragma", "no-cache" },
                { "Cache-Control", "no-cache" }
            };

            res = tlsSession.Post(url, headers, "{}", differentSession: true, proxy: ProxyHelper.FormatProxy(proxy));

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

            return true;
        }

        private string Login(string username, string password, string proxy)
        {
            string url = "https://api.getgrass.io/login";

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
                { "Referer", "https://app.getgrass.io/login" },
                { "Accept-Encoding", "gzip, deflate, br, zstd" },
                { "Accept-Language", "en-US,en;q=0.9" }
            };

            var body = new Dictionary<string, object>
            {
                { "username", username },
                { "password", password }
            };

            var res = tlsSession.Post(url, headers, JObject.FromObject(body).ToString(), differentSession: true, proxy: proxy);

            if (res.Status == 200)
            {
                LoggingHelper.Log("Logged In");

                var jsonBody = JObject.Parse(res.Body);

                return jsonBody["result"]?["data"]?["accessToken"]?.ToString() ?? "";
            }
            else
            {
                Console.WriteLine(res.Body);
                LoggingHelper.Log($"Error logging in ({res.Status})", LoggingHelper.LogType.Error);
                return "";
            }
        }
    }
}
