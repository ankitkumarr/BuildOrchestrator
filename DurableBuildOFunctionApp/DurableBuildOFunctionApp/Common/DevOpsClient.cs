using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DurableBuildOFunctionApp.Common
{
    // Inspired (a.k.a shamelessly copied) from https://github.com/Azure/azure-functions-core-tools/blob/dev/src/Azure.Functions.Cli/Arm/ArmClient.cs
    static class DevOpsClient
    {
        public static async Task<HttpResponseMessage> HttpInvoke(string method, Uri uri, string bearerToken = null, object objectPayload = null, int retryCount = 3)
        {
            var socketTrials = 10;
            var retries = retryCount;
            while (true)
            {
                try
                {
                    var response = await HttpInvoke(uri, method, bearerToken, objectPayload);

                    if (!response.IsSuccessStatusCode && retryCount > 0)
                    {
                        while (retries > 0)
                        {
                            response = await HttpInvoke(uri, method, bearerToken, objectPayload);
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

        private static async Task<HttpResponseMessage> HttpInvoke(Uri uri, string verb, string bearerToken, object objectPayload)
        {
            var payload = JsonConvert.SerializeObject(objectPayload);
            using (var client = Utilities.GetHttpClient(uri, bearerToken))
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
    }
}
