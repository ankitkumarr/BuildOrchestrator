using DurableBuildOFunctionApp.Contexts;
using DurableBuildOFunctionApp.Models;
using Microsoft.Extensions.Logging;
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
                return $"{Constants.DevOps.EndPoint}/{organization}/{project}/_apis/build/builds?api-version={Constants.DevOps.ApiVersion}";
            }

            public static string GetArtifactsUrl(string organization, string project, int buildId, string artifactName)
            {
                return $"{Constants.DevOps.EndPoint}/{organization}/{project}/_apis/build/builds/" +
                    $"{buildId}/artifacts/{artifactName}?api-version={Constants.DevOps.ArtifactsApiVersion}";
            }
        }

        public static async Task<DevOpsBuild> QueueDevOpsBuild(DevOpsBuildContext buildContext, string authToken, ILogger logger)
        {
            Uri url = new Uri(Endpoints.GetQueueBuildUrl(buildContext.Organization, buildContext.Project));
            DevOpsBuild devOpsBuild = new DevOpsBuild
            {
                Definition = new Definition
                {
                    Id = buildContext.DefinitionId
                }
            };
            if (buildContext.Parameters.Count != 0)
            {
                devOpsBuild.Parameters = JsonConvert.SerializeObject(buildContext.Parameters);
            }
            var bearerToken = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", authToken)));

            var response = await DevOpsClient.HttpInvoke("POST", url, bearerToken, devOpsBuild, logger);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DevOpsBuild>(responseContent);
        }

        public static async Task<DevOpsBuild> GetBuildStatus(string devOpsBuildUrl, ILogger logger)
        {
            Uri url = new Uri(devOpsBuildUrl);
            var response = await DevOpsClient.HttpInvoke("GET", url, logger: logger);
            return JsonConvert.DeserializeObject<DevOpsBuild>(await response.Content.ReadAsStringAsync());
        }

        public static async Task<DevOpsArtifact> GetBuildArtifact(DevOpsArtifactContext artifactContext, ILogger logger)
        {
            Uri url = new Uri(Endpoints.GetArtifactsUrl(artifactContext.Organization, artifactContext.Project,
                artifactContext.BuildId, artifactContext.ArtifactName));
            var response = await DevOpsClient.HttpInvoke("GET", url, logger: logger);
            return JsonConvert.DeserializeObject<DevOpsArtifact>(await response.Content.ReadAsStringAsync());
        }
    }
}
