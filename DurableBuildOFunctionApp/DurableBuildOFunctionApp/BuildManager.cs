using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DurableBuildOFunctionApp.Common;
using DurableBuildOFunctionApp.Contexts;
using DurableBuildOFunctionApp.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurableBuildOFunctionApp
{
    public static class BuildManager
    {
        [FunctionName("BuildManager")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            DevOpsBuildContext buildContext = new DevOpsBuildContext
            {
                BuildTaskName = "Initial Test",
                Organization = "azfunc",
                Project = "Azure Functions",
                DefinitionId = 20
            };

            DevOpsBuild bb = await context.CallSubOrchestratorAsync<DevOpsBuild>("BuildManager_OrchestrateBuild", buildContext);
            //outputs.Add((await context.CallActivityAsync<DevOpsBuild>("BuildManager_QueueBuild", buildContext)).Url);
            //outputs.Add((await context.CallActivityAsync<DevOpsBuild>("BuildManager_QueueBuild", buildContext)).Url);
            outputs.Add(bb.Url);
            return outputs;
        }

        [FunctionName("BuildManager_OrchestrateBuild")]
        public static async Task<DevOpsBuild> OrchestrateBuild([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            DevOpsBuildContext buildContext = context.GetInput<DevOpsBuildContext>();
            DevOpsBuild build = await context.CallActivityAsync<DevOpsBuild>("BuildManager_QueueBuild", buildContext);
            DateTime expiry = context.CurrentUtcDateTime + TimeSpan.FromMinutes(Constants.PollSettings.expiryMinutes);
            TimeSpan pollingInterval = TimeSpan.FromSeconds(Constants.PollSettings.pollingIntervalSecs);

            while (context.CurrentUtcDateTime < expiry)
            {
                build = await context.CallActivityAsync<DevOpsBuild>("BuildManager_GetBuildStatus", build.Url);
                if (build.Status == BuildStatus.completed)
                {
                    return build;
                }
            }

            // TODO: Need to verify that this is ok
            return build;
        }

        [FunctionName("BuildManager_QueueBuild")]
        public static DevOpsBuild QueueBuild([ActivityTrigger] DevOpsBuildContext buildContext, ILogger log)
        {
            log.LogInformation($"Kicking off a build for {buildContext.BuildTaskName}...");
            string authToken = EnvironmentHelper.GetAuthToken();
            DevOpsBuild build = DevOpsHelper.QueueDevOpsBuild(buildContext, authToken).Result;
            return build;
        }

        [FunctionName("BuildManager_GetBuildStatus")]
        public static DevOpsBuild PollBuild([ActivityTrigger] string buildUrl, ILogger log)
        {
            log.LogInformation($"Polling the build URL: {buildUrl}...");
            DevOpsBuild build = DevOpsHelper.GetBuildStatus(buildUrl).Result;
            return build;
        }

        [FunctionName("BuildManager_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("BuildManager", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}