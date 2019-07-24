using DurableBuildOFunctionApp.Common;
using System;
using System.Collections.Generic;
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
            var buildContext = new DevOpsBuildContext
            {
                BuildTaskName = $"Running build for {worker}",
                Organization = EnvironmentHelper.GetOrganization(),
                Project = EnvironmentHelper.GetProject(),
                DefinitionId = EnvironmentHelper.GetDefinitionId(prefix),
                Parameters = EnvironmentHelper.GetPrefixVariables(prefix)
            };

            return buildContext;
        }

        private static DevOpsBuildContext PlatformBuildContext(string platform)
        {
            var prefix = Constants.PlatformConfigToPrefix[platform];
            var buildContext = new DevOpsBuildContext
            {
                BuildTaskName = $"Setup for {platform}",
                Organization = EnvironmentHelper.GetOrganization(),
                Project = EnvironmentHelper.GetProject(),
                DefinitionId = EnvironmentHelper.GetDefinitionId(prefix),
                Parameters = EnvironmentHelper.GetPrefixVariables(prefix)
            };

            return buildContext;
        }
    }
}
