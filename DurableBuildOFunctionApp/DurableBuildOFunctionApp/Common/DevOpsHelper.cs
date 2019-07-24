using DurableBuildOFunctionApp.Contexts;
using DurableBuildOFunctionApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DurableBuildOFunctionApp.Common
{
    public static class DevOpsHelper
    {
        public static class Endpoints
        {
            public static string GetQueueBuildUrl(string organization, string project)
            {
                return $"{Constants.DevOps.DevOpsEndPoint}/{organization}/{project}/_apis/build/builds?api-version={Constants.DevOps.DevOpsApiVersion}";
            }
        }

        public static async Task<DevOpsBuild> QueueDevOpsBuild(DevOpsBuildContext buildContext, string authToken)
        {
            Uri url = new Uri(Endpoints.GetQueueBuildUrl(buildContext.Organization, buildContext.Project));
            DevOpsBuild devOpsBuild = new DevOpsBuild
            {
                Definition = new Definition
                {
                    Id = buildContext.DefinitionId
                },
                Parameters = JsonConvert.SerializeObject(buildContext.Parameters)
            };
            var bearerToken = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", authToken)));

            var response = await DevOpsClient.HttpInvoke("POST", url, bearerToken, devOpsBuild);
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DevOpsBuild>(responseContent);
        }

        public static async Task<DevOpsBuild> GetBuildStatus(string devOpsBuildUrl)
        {
            Uri url = new Uri(devOpsBuildUrl);
            var response = await DevOpsClient.HttpInvoke("GET", url);
            return JsonConvert.DeserializeObject<DevOpsBuild>(await response.Content.ReadAsStringAsync());
        }
    }
}
