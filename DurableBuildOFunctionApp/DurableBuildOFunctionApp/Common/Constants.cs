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
        }

        public static class PollSettings
        {
            public const int pollingIntervalSecs = 15;
            public const int expiryMinutes = 60;
        }
    }
}
