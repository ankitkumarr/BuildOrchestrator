using System;
using System.Collections.Generic;
using System.Text;

namespace DurableBuildOFunctionApp.Common
{
    public static class Constants
    {
        public static class DevOps
        {
            public static readonly string DevOpsEndPoint = "https://dev.azure.com";
            public static readonly string DevOpsApiVersion = "5.1";
        }

        public static class EnvVars
        {
            public static readonly string AuthToken = "DEVOPS_AUTH_TOKEN";
            public static readonly string DevOpsProject = "DEVOPS_PROJECT";
            public static readonly string DevOpsOrg = "DEVOPS_ORGANIZATION";
            public static readonly string DefinitionIDSuffix = "DEFINITION_ID";
        }

        public static class PollSettings
        {
            public const int pollingIntervalSecs = 15;
            public const int expiryMinutes = 60;
        }

        public static readonly IReadOnlyDictionary<string, string> WorkerConfigToPrefix = new Dictionary<string, string>
        {
            { "node", "DEVOPS_NODE_" },
            { "java", "DEVOPS_JAVA_" },
            { "python", "DEVOPS_PYTHON_" },
            { "dotnet", "DEVOPS_DOTNET_" }
        };

        public static readonly IReadOnlyDictionary<string, string> PlatformConfigToPrefix = new Dictionary<string, string>
        {
            { "windows", "DEVOPS_WIN_" },
            { "linux", "DEVOPS_LINUX_" }
        };

        public static readonly IEnumerable<string> LanguageWorkersForBuild = WorkerConfigToPrefix.Keys;
        public static readonly IEnumerable<string> PlatformsForSetup = PlatformConfigToPrefix.Keys;

    }
}
