using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DawnAccountGen.Utils
{
    internal class CapsolverHandler
    {
        private static readonly string fileName = MethodBase.GetCurrentMethod()!.DeclaringType!.ToString();
        private TLSSession Session;
        private readonly string ApiPath = "https://api.capsolver.com/";
        private string SoftId = "952E845C-6D59-4FE6-8348-6ED555ADE469";
        private string ApiKey = string.Empty;

        public CapsolverHandler()
        {
            Session = new TLSSession();
        }

        public void SetSoftId(string softId)
        {
            SoftId = softId;
        }

        public void SetApiKey(string apiKey)
        {
            ApiKey = apiKey;
        }

        public string CreateTask(string type, string websiteUrl, string websiteKey, string? userAgent = null, bool? isInvisible = null, string? s = null, string? pageAction = null)
        {
            // Create a Task to solve Captchas
            // Make the json body for the request.
            var JsonBody = new Dictionary<string, object>
            {
                {"clientKey", ApiKey },
                {"appId", SoftId },
                {"task", new Dictionary<string,object>
                    {
                        {"type", type },
                        {"websiteURL", websiteUrl },
                        {"websiteKey", websiteKey }
                    }
                }
            };

            if (userAgent != null)
            {
                ((Dictionary<string, object>)JsonBody["task"]).Add("userAgent", userAgent);
            }

            if (isInvisible != null)
            {
                ((Dictionary<string, object>)JsonBody["task"]).Add("isInvisible", isInvisible);
            }

            if (s != null)
            {
                ((Dictionary<string, object>)JsonBody["task"]).Add("enterprisePayload", new Dictionary<string, string>());
                ((Dictionary<string, string>)((Dictionary<string, object>)JsonBody["task"])["enterprisePayload"]).Add("s", s);
            }

            if (pageAction != null)
            {
                ((Dictionary<string, object>)JsonBody["task"]).Add("pageAction", pageAction);
            }

            RequestResult Response = Session.Post(ApiPath + "createTask", new Dictionary<string, string> { { "Content-Type", "application/json" } }, JObject.FromObject(JsonBody).ToString());

            if (Response.Status != 200)
            {
                Console.WriteLine(fileName + " | Failed to create task: " + Response.Body);
                return string.Empty;
            }
            else
            {
                //Json Response
                JObject Json = JObject.Parse(Response.Body);
                return Json["taskId"].ToString();
            }
        }

        public async Task<JObject?> GetTaskResults(string taskId)
        {
            // Create a Task to solve Captchas
            // Make the json body for the request.
            var JsonBody = new Dictionary<string, string>
            {
                {"clientKey", ApiKey },
                {"taskId", taskId}
            };

            RequestResult Response = Session.Post(ApiPath + "getTaskResult", null, JObject.FromObject(JsonBody).ToString());

            if (Response.Status != 200)
            {
                Console.WriteLine(fileName + " | Failed to get task results: " + Response.Body);
                return null;
            }
            else
            {
                var Json = JObject.Parse(Response.Body);
                while (Json["status"].ToString() != "ready" && Json["errorCode"] == null)
                {
                    await System.Threading.Tasks.Task.Delay(3000);
                    Response = Session.Post(ApiPath + "getTaskResult", null, JObject.FromObject(JsonBody).ToString());
                    Json = JObject.Parse(Response.Body);
                }

                return Json;
            }
        }

        public async Task<string?> GetTurnstile(string url, string key)
        {

            string TaskId = CreateTask("AntiTurnstileTaskProxyLess", url, key);

            if (String.IsNullOrWhiteSpace(TaskId))
            {               
                return null;
            }


            JObject? TaskResults = await GetTaskResults(TaskId).ConfigureAwait(false);

            if (TaskResults == null)
            {
                return null;
            }


            if (TaskResults["errorCode"] != null)
            {
                Console.WriteLine(fileName + " | Failed to get task results: " + TaskResults.ToString());
                return null;
            }

            return TaskResults["solution"]["token"].ToString();
        }

        public async Task<string?> GetRecaptchaV2Enterprise(string url, string key)
        {

            string TaskId = CreateTask("ReCaptchaV2EnterpriseTaskProxyLess", url, key, isInvisible: true);

            if (String.IsNullOrWhiteSpace(TaskId))
            {
                return null;
            }


            JObject? TaskResults = await GetTaskResults(TaskId).ConfigureAwait(false);

            if (TaskResults == null)
            {
                return null;
            }


            if (TaskResults["errorCode"] != null)
            {
                Console.WriteLine(fileName + " | Failed to get task results: " + TaskResults.ToString());
                return null;
            }

            return TaskResults["solution"]["gRecaptchaResponse"].ToString();
        }
    }
}
