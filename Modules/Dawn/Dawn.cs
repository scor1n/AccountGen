using AccountGen.Services;
using AccountGen.Utils;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Math;
using RandomNameGeneratorLibrary;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AccountGen.Modules.Dawn
{
    internal class Dawn
    {
        private readonly CapsolverHandler caphandler = new();
        private readonly TLSSession tlsSession = new();
        private readonly Random random = new();
        private readonly PersonNameGenerator nameGenerator = new();
        private readonly string catchallDomain = SettingsHelper.GetCatchallDomain();
        private readonly string capsolverKey = SettingsHelper.GetCapsolverKey();
        private readonly string twoLetterCountryCode = SettingsHelper.GetTwoLetterCountryCode();
        private readonly string dawnTurnstileKey = "0x4AAAAAAA48wVDquA-98fyV";
        private readonly string dawnExtensionId = "fpdkjdnhkakefebpekbdhillbhonfjjp";

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
            List<string> accounts = [];
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

            var filePath = $"{SettingsHelper.GetOutputFolder()}dawnAccounts-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.csv";
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

            var tokenTask = caphandler.GetTurnstile("https://dashboard.dawninternet.com/signup", dawnTurnstileKey);
            tokenTask.Wait();
            var token = tokenTask.Result ?? "";

            string url = "https://ext-api.dawninternet.com/chromeapi/dawn/v2/dashboard/user/validate-register";

            var headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            };

            var names = nameGenerator.GenerateRandomFirstAndLastName().Split(' ');
            if (email.Length == 0)
            {
                email = $"{names[0]}.{names[1]}{random.Next(99)}@{catchallDomain}";
            }
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
                if (jsonBody["success"]?.Value<bool?>() == false)
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
            var verificationId = IMAPService.GetInstance().GetVerificationIdFromEmailDawn(username);
            
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
                if (jsonBody["success"]?.Value<bool?>() == false)
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

        internal void GetReferralCodes()
        {
            var loggedInAccountsCsvPath = SettingsHelper.GetInputLoggedInAccountsFile();

            var records = new List<Dictionary<string, string>>();

            string[] lines = File.ReadAllLines(loggedInAccountsCsvPath);
            string[] headers = lines[0].Split(',');

            string[] requiredHeaders = ["username", "password", "proxy", "app_id", "bearer_token"];

            bool hasAllHeaders = requiredHeaders.All(h => headers.Contains(h));

            if (!hasAllHeaders)
            {
                LoggingHelper.Log("CSV is missing required headers!", LoggingHelper.LogType.Error);
                return;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                var record = new Dictionary<string, string>();

                for (int j = 0; j < headers.Length; j++)
                {
                    record[headers[j]] = values[j];
                }

                records.Add(record);
            }

            Dictionary<string, string> emailToReferralCode = [];

            LoggingHelper.Log("Starting to get referral codes");

            for (int i = 1; i < records.Count; i++)
            {
                try
                {
                    var currentRecord = records[i];

                    string referralCode = "";
                    int tries = 0;

                    string bearerToken = currentRecord["bearer_token"];
                    string appId = currentRecord["app_id"];
                    string proxy = currentRecord["proxy"];
                    string username = currentRecord["username"];

                    do
                    {
                        referralCode = GetReferralCode(bearerToken, appId, proxy);
                        tries++;
                    } while (string.IsNullOrWhiteSpace(referralCode) && tries < 5);

                    if (string.IsNullOrWhiteSpace(referralCode))
                    {
                        LoggingHelper.Log($"Failed to get referral code for account {username} 5 times!", LoggingHelper.LogType.Error);
                    }
                    else
                    {
                        LoggingHelper.Log($"Got referral code for {username}", LoggingHelper.LogType.Success);
                        emailToReferralCode.Add(username, referralCode);
                    }
                }
                catch (Exception ex) 
                {
                    LoggingHelper.Log($"Failed to get referal code with error: {ex.Message}", LoggingHelper.LogType.Error);
                }
            }

            LoggingHelper.Log("Finished grabbing referral codes, writing to output file", LoggingHelper.LogType.Success);

            var csvRows = emailToReferralCode.Select(kv => $"{kv.Key},{kv.Value}").ToList();

            csvRows.Insert(0, "username,referral_code");

            var filePath = $"{SettingsHelper.GetOutputFolder()}dawnReferralCodes-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.csv";
            
            File.WriteAllLines(filePath, csvRows);

            LoggingHelper.Log("Saved referral codes to file!", LoggingHelper.LogType.Success);
        }

        private string GetReferralCode(string bearerToken, string appId, string proxy)
        {
            string url = $"https://www.aeropres.in/api/atom/v1/userreferral/getpoint?appid={appId}";

            var headers = new Dictionary<string, string>
            {
                { "sec-ch-ua-platform", "\"Windows\"" },
                { "Authorization", bearerToken },
                { "User-Agent", tlsSession.UserAgent },
                { "sec-ch-ua", "\"Not(A:Brand\";v=\"99\", \"Google Chrome\";v=\"129\", \"Chromium\";v=\"129\"" },
                { "DNT", "1" },
                { "Content-Type", "application/json" },
                { "sec-ch-ua-mobile", "?0" },
                { "Accept", "*/*" },
                { "Origin", $"chrome-extension://{dawnExtensionId}" },
                { "Sec-Fetch-Site", "cross-site" },
                { "Sec-Fetch-Mode", "cors" },
                { "Sec-Fetch-Dest", "empty" },
                { "Accept-Encoding", "gzip, deflate, br, zstd" },
                { "Accept-Language", "en-US,en;q=0.9,fr;q=0.8" },
                { "Pragma", "no-cache" },
                { "Cache-Control", "no-cache" }
            };

            var res = tlsSession.Get(url, headers, differentSession: true, proxy: ProxyHelper.FormatProxy(proxy));

            if (res == null)
            {
                LoggingHelper.Log("Response was null", LoggingHelper.LogType.Error);
                return "";
            }

            if (res.Status != 200)
            {
                LoggingHelper.Log($"Status Code {res.Status}", LoggingHelper.LogType.Error);
                return "";
            }

            try
            {
                var jsonBody = JObject.Parse(res.Body);
                if (jsonBody["status"]?.Value<bool?>() == false)
                {
                    LoggingHelper.Log("Got the following response when getting referral code: " + jsonBody.ToString(), LoggingHelper.LogType.Error);
                    return "";
                } 
                else if (jsonBody["message"]?.Value<string?>() != "success")
                {
                    LoggingHelper.Log("Got the following response when getting referral code: " + jsonBody.ToString(), LoggingHelper.LogType.Error);
                    return "";
                }

                return jsonBody["data"]?["referralPoint"]?["referralCode"]?.ToString() ?? "";
            }
            catch (Exception ex)
            {
                LoggingHelper.Log("Error checking response body", LoggingHelper.LogType.Error);
                LoggingHelper.Log(ex.ToString());
                return "";
            }
        }
    }
}
