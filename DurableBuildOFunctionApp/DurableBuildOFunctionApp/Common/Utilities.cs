using DurableBuildOFunctionApp.Contexts;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace DurableBuildOFunctionApp.Common
{
    class Utilities
    {
        public static HttpClient GetHttpClient(Uri url, string bearerToken = null, HttpMessageHandler handler = null, TimeSpan? timeout = null)
        {
            var client = new HttpClient(handler ?? new HttpClientHandler())
            {
                BaseAddress = url,
                Timeout = timeout ?? Timeout.InfiniteTimeSpan
            };
            // client.DefaultRequestHeaders.Add("User-Agent", "functions-build-orchestrator/v0.1");
            if (bearerToken != null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", bearerToken);
            }
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            return client;
        }

        public static string BuildTypeToCon(BuildType buildtype)
        {
            switch(buildtype)
            {
                case BuildType.WorkerSite:
                    return "site";
                case BuildType.WorkerCli:
                    return "cli";
                case BuildType.SiteSetup:
                    return "set";
                default:
                    return "co";
            }
        }
    }
}
