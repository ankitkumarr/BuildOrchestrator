using System;
using System.Collections.Generic;
using System.Linq;
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

            // Run the worker CLI tasks in parallel
            var workerCliTasks = new List<Task<DevOpsBuild>>();
            foreach (var workerCliContext in ContextProvider.GetWorkerCliBuildContexts())
            {
                Task<DevOpsBuild> workerCliBuild = context.CallSubOrchestratorAsync<DevOpsBuild>("BuildManager_OrchestrateBuild", workerCliContext);
                workerCliTasks.Add(workerCliBuild);
            }
            var workerCliBuilds = await Task.WhenAll(workerCliTasks);
            outputs.AddRange(workerCliBuilds.Select(b => b.Url));

            // Get the platform main setup ready
            var setupTasks = new List<Task<DevOpsBuild>>();
            foreach (var setupContext in ContextProvider.GetSetupBuildContexts())
            {
                Task<DevOpsBuild> setupBuild = context.CallSubOrchestratorAsync<DevOpsBuild>("BuildManager_OrchestrateBuild", setupContext);
                setupTasks.Add(setupBuild);
            }
            var setupBuilds = await Task.WhenAll(setupTasks);
            outputs.AddRange(setupBuilds.Select(b => b.Url));

            // Run the worker Site tasks in parallel
            var workerSiteTasks = new List<Task<DevOpsBuild>>();
            foreach (var workerSiteContext in ContextProvider.GetWorkerSiteBuildContexts())
            {
                Task<DevOpsBuild> workerSiteBuild = context.CallSubOrchestratorAsync<DevOpsBuild>("BuildManager_OrchestrateBuild", workerSiteContext);
                workerSiteTasks.Add(workerSiteBuild);
            }
            var workerSiteBuilds = await Task.WhenAll(workerSiteTasks);
            outputs.AddRange(workerSiteBuilds.Select(b => b.Url));

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
                    // For worker builds we need to get the artifacts and upload it
                    if (Constants.LanguageWorkersForBuild.Contains(buildContext.Agent)
                        && EnvironmentHelper.ShouldUploadArtifact())
                    {
                        var artifactContext = ContextProvider.GetWorkerArtifactContext(build.Id, buildContext.Agent);
                        var artifact = await context.CallActivityAsync<DevOpsArtifact>("BuildManager_GetBuildArtifact", artifactContext);

                        // TODO: change the platform here
                        var uploadContext = ContextProvider.GetWorkerArtifactUploadContext(artifact, 
                            $"{Utilities.BuildTypeToCon(buildContext.BuildType)}", buildContext.Agent);

                        await context.CallActivityAsync("BuildManager_UploadToStorage", uploadContext);
                    }
                    return build;
                }

                // Orchestration sleeps until this time.
                var nextCheck = context.CurrentUtcDateTime + pollingInterval;
                await context.CreateTimer(nextCheck, CancellationToken.None);
            }

            // TODO: Need to verify that this is ok
            // and probably throw a "failure" log
            return build;
        }

        [FunctionName("BuildManager_QueueBuild")]
        public static DevOpsBuild QueueBuild([ActivityTrigger] DevOpsBuildContext buildContext, ILogger log)
        {
            log.LogInformation($"Kicking off a build for {buildContext.BuildTaskName}...");
            string authToken = EnvironmentHelper.GetAuthToken();
            DevOpsBuild build = DevOpsHelper.QueueDevOpsBuild(buildContext, authToken, log).Result;
            return build;
        }

        [FunctionName("BuildManager_GetBuildStatus")]
        public static DevOpsBuild PollBuild([ActivityTrigger] string buildUrl, ILogger log)
        {
            log.LogInformation($"Polling the build URL: {buildUrl}...");
            DevOpsBuild build = DevOpsHelper.GetBuildStatus(buildUrl, log).Result;
            return build;
        }

        [FunctionName("BuildManager_GetBuildArtifact")]
        public static DevOpsArtifact GetBuildArtifact([ActivityTrigger] DevOpsArtifactContext artifactContext, ILogger log)
        {
            log.LogInformation($"Getting the build artifact info for {artifactContext.Agent}...");
            DevOpsArtifact artifact = DevOpsHelper.GetBuildArtifact(artifactContext, log).Result;
            return artifact;
        }

        [FunctionName("BuildManager_UploadToStorage")]
        public static void UploadToStorage([ActivityTrigger] BlobUploadContext uploadContext, ILogger log)
        {
            log.LogInformation($"Uploading the test artifacts for worker {uploadContext.Worker}...");
            StorageHelper.UploadArtifactToStorage(uploadContext).Wait();
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