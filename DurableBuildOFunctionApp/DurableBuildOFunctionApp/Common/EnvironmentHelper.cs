using System;
using System.Collections.Generic;
using System.Text;

namespace DurableBuildOFunctionApp.Common
{
    public static class EnvironmentHelper
    {
        public static string GetAuthToken()
        {
            var authToken = Environment.GetEnvironmentVariable(Constants.EnvVars.AuthToken);
            if (string.IsNullOrEmpty(authToken))
            {
                throw new KeyNotFoundException($"Access token not found. Please set {Constants.EnvVars.AuthToken}");
            }
            return authToken;
        }
    }
}
