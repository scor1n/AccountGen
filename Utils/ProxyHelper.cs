using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DawnAccountGen.Utils
{
    internal static class ProxyHelper
    {
        internal static List<string> GetProxies()
        {
            var proxyList = new List<string>();
            var unformatedList = File.ReadAllLines(SettingsHelper.GetInputProxyFile());
            for (int i = 0; i < unformatedList.Length; i++)
            {
                var rawProxyString = unformatedList[i].Trim();
                var rawProxySplit = rawProxyString.Split(':');
                var formatedProxy = $"http://{rawProxySplit[2]}:{rawProxySplit[3]}@{rawProxySplit[0]}:{rawProxySplit[1]}";
                proxyList.Add(formatedProxy);
            }
            return proxyList;
        }

        internal static string UnformatProxy(string proxy)
        { 
            proxy = proxy.Remove(0, 7); // Remove http://
            var proxySplit = proxy.Split("@");
            return $"{proxySplit[1]}:{proxySplit[0]}";
        }
    }
}
