using DurableBuildOFunctionApp.Common;
using DurableBuildOFunctionApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DurableBuildOFunctionApp.Contexts
{
    public static class ContextProvider
    {
        public static IList<DevOpsBuildContext> GetSetupBuildContexts()
        {
            IList<DevOpsBuildContext> buildContexts = new List<DevOpsBuildContext>();

            foreach (var platform in Constants.PlatformsForSetup)
            {
                buildContexts.Add(PlatformBuildContext(platform));
            }

            return buildContexts;
        }

        public static IList<DevOpsBuildContext> GetWorkerBuildContexts()
        {
            IList<DevOpsBuildContext> buildContexts = new List<DevOpsBuildContext>();

            foreach (var worker in Constants.LanguageWorkersForBuild)
            {
                buildContexts.Add(WorkerBuildContext(worker));
            }

            return buildContexts;
        }

        private static DevOpsBuildContext WorkerBuildContext(string worker)
        {
            var prefix = Constants.WorkerConfigToPrefix[worker];
            var workerParameters = EnvironmentHelper.GetPrefixVariables(prefix);
            var parameters = EnvironmentHelper.GetPrefixVariables(Constants.EnvVars.WorkerCommonPrefix);
            workerParameters.ToList().ForEach(x => parameters[x.Key] = x.Value);

            var buildContext = new DevOpsBuildContext
            {
                BuildTaskName = $"Running build for {worker}",
                Agent = worker,
                Organization = EnvironmentHelper.GetOrganization(),
                Project = EnvironmentHelper.GetProject(worker),
                DefinitionId = EnvironmentHelper.GetDefinitionId(prefix),
                Parameters = parameters
            };

            return buildContext;
        }

        private static DevOpsBuildContext PlatformBuildContext(string platform)
        {
            var prefix = Constants.PlatformConfigToPrefix[platform];
            var platformParameters = EnvironmentHelper.GetPrefixVariables(prefix);
            var parameters = EnvironmentHelper.GetPrefixVariables(Constants.EnvVars.PlatformCommonPrefix);
            platformParameters.ToList().ForEach(x => parameters[x.Key] = x.Value);
            var buildContext = new DevOpsBuildContext
            {
                BuildTaskName = $"Setup for {platform}",
                Agent = platform,
                Organization = EnvironmentHelper.GetOrganization(),
                Project = EnvironmentHelper.GetProject(),
                DefinitionId = EnvironmentHelper.GetDefinitionId(prefix),
                Parameters = parameters
            };

            return buildContext;
        }

        public static DevOpsArtifactContext GetWorkerArtifactContext(int buildId, string worker)
        {
            var workerPrefix = Constants.WorkerConfigToPrefix[worker];
            var commonPrefix = Constants.EnvVars.WorkerCommonPrefix;
            var artifactContext = new DevOpsArtifactContext
            {
                ArtifactTaskName = $"Artifacts for {worker}",
                Organization = EnvironmentHelper.GetOrganization(),
                Project = EnvironmentHelper.GetProject(worker),
                BuildId = buildId,
                ArtifactName = EnvironmentHelper.GetArtifactName(workerPrefix, commonPrefix)
            };
            return artifactContext;
        }

        public static BlobUploadContext GetWorkerArtifactUploadContext(DevOpsArtifact artifact, string platform, string worker)
        {
            var workerPrefix = Constants.WorkerConfigToPrefix[worker];
            var commonPrefix = Constants.EnvVars.WorkerCommonPrefix;
            var uploadContext = new BlobUploadContext
            {
                Worker = worker,
                Artifact = artifact,
                Platform = platform,
                Environment = EnvironmentHelper.GetHostEnvironment()
            };
            return uploadContext;
        }
    }
}
