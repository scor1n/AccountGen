using Newtonsoft.Json;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace AccountGen.Utils
{
    public class RequestResult
    {
        public string Id { get; set; }
        public string Body { get; set; }
        public dynamic Cookies { get; set; }
        public Dictionary<string, List<string>> Headers { get; set; }
        public int Status { get; set; }
        public string Target { get; set; }
        public string UsedProtocol { get; set; }
    }

    public class RequestPayload
    {
        public string TlsClientIdentifier { get; set; } = "";
        public bool FollowRedirects { get; set; } = true;
        public bool InsecureSkipVerify { get; set; } = false;
        public bool WithoutCookieJar { get; set; } = false;
        public bool WithDefaultCookieJar { get; set; } = true;
        public bool IsByteRequest { get; set; } = false;
        public bool ForceHttp1 { get; set; } = false;
        public bool WithDebug { get; set; } = false;
        public bool CatchPanics { get; set; } = false;
        public bool WithRandomTLSExtensionOrder { get; set; } = false;
        public string sessionId { get; set; } = Guid.NewGuid().ToString();
        public int TimeoutSeconds { get; set; } = 60;
        public int TimeoutMilliseconds { get; set; } = 0;
        public Dictionary<string, string> CertificatePinningHosts { get; set; } = new Dictionary<string, string>();
        public string ProxyUrl { get; set; } = "";
        public bool IsRotatingProxy { get; set; } = true;
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public List<string> HeaderOrder { get; set; }
        public string RequestUrl { get; set; }
        public string RequestMethod { get; set; }
        public string RequestBody { get; set; }
    }

    public class TLSSession
    {
        [DllImport("dlls/tls-client-1.7.11", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern IntPtr request(byte[] requestPayload, string sessionID);

        [DllImport("dlls/tls-client-1.7.11", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern IntPtr addCookiesToSession(byte[] requestPayload, string sessionID);

        [DllImport("dlls/tls-client-1.7.11", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern IntPtr getCookiesFromSession(byte[] requestPayload, string sessionID);

        [DllImport("dlls/tls-client-1.7.11", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void freeMemory(string responseId);

        private static readonly string fileName = MethodBase.GetCurrentMethod().DeclaringType.ToString();
        private string sessionID;
        public RequestPayload sessionPayload;

        public string Proxy { get; set; } = "";
        public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36";

        public Dictionary<string, string> defaultHeaders = new Dictionary<string, string>() { { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36" } };

        public TLSSession(Dictionary<string, string> headers = null, string TlsClientIdentifier = "chrome_124", int TimeoutSeconds = 60, bool FollowRedirects = false, string proxy = null)
        {

            this.sessionID = Guid.NewGuid().ToString();

            this.sessionPayload = new RequestPayload
            {
                TlsClientIdentifier = TlsClientIdentifier,
                FollowRedirects = FollowRedirects,
                InsecureSkipVerify = false,
                IsByteRequest = false,
                ForceHttp1 = false,
                WithDebug = false,
                CatchPanics = false,
                WithRandomTLSExtensionOrder = true,
                sessionId = this.sessionID,
                TimeoutSeconds = TimeoutSeconds,
                TimeoutMilliseconds = 0,
                CertificatePinningHosts = new Dictionary<string, string>(),
                ProxyUrl = "",
                IsRotatingProxy = true,
                Headers = headers ?? new Dictionary<string, string>() { { "user-agent", UserAgent } },
                HeaderOrder = headers != null ? new List<string>(headers.Keys) : new List<string>() { "user-agent" },
                RequestUrl = "",
                RequestMethod = "",
                RequestBody = "",
            };
            if (proxy != null)
            {
                this.sessionPayload.ProxyUrl = proxy;
                this.Proxy = proxy;
            }
        }

        public RequestResult Get(string url, Dictionary<string, string> additionalHeaders = null, Dictionary<string, string> headers = null, bool differentSession = false, bool withoutProxy = false, string proxy = "")
        {
            return this.MakeRequest("GET", url, MergeHeaders(headers ?? this.sessionPayload.Headers, additionalHeaders), "", differentSession, proxy);
        }

        public RequestResult Post(string url, Dictionary<string, string> additionalHeaders = null, string body = "", Dictionary<string, string> headers = null, bool differentSession = false, bool withoutProxy = false, string proxy = "")
        {
            return this.MakeRequest("POST", url, MergeHeaders(headers ?? this.sessionPayload.Headers, additionalHeaders), body, differentSession, proxy);
        }

        private RequestResult MakeRequest(string method, string url, Dictionary<string, string> headers, string body = "", bool withDifferentSession = false, string proxy = "")
        {
            var newSessionPayload = this.sessionPayload;

            if (withDifferentSession) 
            {
                newSessionPayload.sessionId = Guid.NewGuid().ToString();
            }

            if (!string.IsNullOrEmpty(proxy))
            {
                newSessionPayload.ProxyUrl = proxy;
            }

            newSessionPayload.RequestMethod = method;
            newSessionPayload.RequestUrl = url;
            newSessionPayload.Headers = headers;
            newSessionPayload.RequestBody = body;
            newSessionPayload.HeaderOrder = new List<string>(headers.Keys);

            string requestJson = JsonConvert.SerializeObject(newSessionPayload);
            byte[] requestBytes = Encoding.UTF8.GetBytes(requestJson);

            IntPtr responsePtr = request(requestBytes, newSessionPayload.sessionId);
            string responseJson = Marshal.PtrToStringAnsi(responsePtr);

            RequestResult result = JsonConvert.DeserializeObject<RequestResult>(responseJson);

            int retry = 0;

            while (result.Status == 0 && retry < 3)
            {
                if (retry < 3)
                {
                    retry++;
                    Console.WriteLine($"Retrying {method} request to {url} {retry}/3");
                    freeMemory(result.Id);
                    Thread.Sleep(500);
                    responsePtr = request(requestBytes, newSessionPayload.sessionId);
                    responseJson = Marshal.PtrToStringAnsi(responsePtr);
                    result = JsonConvert.DeserializeObject<RequestResult>(responseJson);
                }
            }

            freeMemory(result.Id);

            return result;
        }

        private Dictionary<string, string> MergeHeaders(Dictionary<string, string> originalHeaders, Dictionary<string, string> additionalHeaders)
        {
            if (additionalHeaders == null) return originalHeaders;

            foreach (var header in additionalHeaders)
            {
                originalHeaders[header.Key] = header.Value;
            }

            return originalHeaders;
        }
    }
}