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
        public static HttpClient GetHttpClient(Uri url, string bearerToken = null, TimeSpan? timeout = null)
        {
            var handler = new HttpClientHandler();
            var client = new HttpClient(handler)
            {
                BaseAddress = url,
                Timeout = timeout ?? Timeout.InfiniteTimeSpan
            };
            client.DefaultRequestHeaders.Add("User-Agent", "functions-build-orchestrator/v0.1");
            if (bearerToken != null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", bearerToken);
            }
            return client;
        }
    }
}
