using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DurableBuildOFunctionApp.Common
{
    // Inspired (a.k.a shamelessly copied) from https://github.com/Azure/azure-functions-core-tools/blob/dev/src/Azure.Functions.Cli/Arm/ArmClient.cs
    static class DevOpsClient
    {
        public static async Task<HttpResponseMessage> HttpInvoke(string method, Uri uri, string bearerToken = null, object objectPayload = null, ILogger logger = null, int retryCount = 3)
        {
            var socketTrials = 10;
            var retries = retryCount;
            while (true)
            {
                try
                {
                    var response = await HttpInvoke(uri, method, bearerToken, objectPayload, logger);

                    if (!response.IsSuccessStatusCode && retryCount > 0)
                    {
                        while (retries > 0)
                        {
                            response = await HttpInvoke(uri, method, bearerToken, objectPayload, logger);
                            if (response.IsSuccessStatusCode)
                            {
                                return response;
                            }
                            else
                            {
                                retries--;
                            }
                        }
                    }
                    return response;
                }
                catch (SocketException)
                {
                    if (socketTrials <= 0) throw;
                    socketTrials--;
                }
                catch (Exception)
                {
                    if (retries <= 0) throw;
                    retries--;
                }
                await Task.Delay(3000);
            }
        }

        private static async Task<HttpResponseMessage> HttpInvoke(Uri uri, string verb, string bearerToken, object objectPayload, ILogger logger)
        {
            var payload = JsonConvert.SerializeObject(objectPayload);
            HttpMessageHandler httpHandler = new HttpClientHandler();
            if (logger != null)
            {
                httpHandler = new LoggingHandler(httpHandler, logger);
            }
            using (var client = Utilities.GetHttpClient(uri, bearerToken, httpHandler))
            {
                const string jsonContentType = "application/json";
                HttpResponseMessage response = null;
                if (string.Equals(verb, "get", StringComparison.OrdinalIgnoreCase))
                {
                    response = await client.GetAsync(uri);
                }
                else if (string.Equals(verb, "post", StringComparison.OrdinalIgnoreCase))
                {
                    response = await client.PostAsync(uri, new StringContent(payload ?? String.Empty, Encoding.UTF8, jsonContentType));
                }
                else
                {
                    throw new InvalidOperationException(String.Format("Invalid http verb '{0}'!", verb));
                }
                return response;
            }
        }

        // https://stackoverflow.com/a/18925296
        public class LoggingHandler : DelegatingHandler
        {
            private ILogger Logger;
            public LoggingHandler(HttpMessageHandler innerHandler, ILogger logger)
                : base(innerHandler)
            {
                Logger = logger;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Logger.LogInformation("Request:");
                Logger.LogInformation(request.ToString());
                if (request.Content != null)
                {
                    Logger.LogInformation(await request.Content.ReadAsStringAsync());
                }

                HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

                Logger.LogInformation("Response:");
                Logger.LogInformation(response.ToString());
                if (response.Content != null)
                {
                    Logger.LogInformation(await response.Content.ReadAsStringAsync());
                }

                return response;
            }
        }
    }
}
