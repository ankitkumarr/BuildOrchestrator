using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DurableBuildOFunctionApp.Common
{
    public static class EnvironmentHelper
    {
        public static string GetAuthToken()
        {
            return GetEnvironment(Constants.EnvVars.AuthToken);
        }

        public static string GetOrganization()
        {
            return GetEnvironment(Constants.EnvVars.DevOpsOrg);
        }

        public static string GetProject()
        {
            return GetEnvironment(Constants.EnvVars.DevOpsProject);
        }

        public static int GetDefinitionId(string prefix)
        {
            var defIdStr = GetEnvironment($"{prefix}{Constants.EnvVars.DefinitionIDSuffix}");

            if (int.TryParse(defIdStr, out int defId))
            {
                return defId;
            }

            throw new FormatException($"Definition ID {prefix}{Constants.EnvVars.DefinitionIDSuffix} must be an integer.");
        }

        public static IDictionary<string, string> GetPrefixVariables(string prefix)
        {
            IDictionary<string, string> variables = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> kp in variables)
            {
                // If settings start with this, we strip the prefix out and record the value
                if (kp.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    variables[kp.Key.Substring(prefix.Length)] = kp.Value;
                }
            }

            return variables;
        }

        private static string GetEnvironment(string varName, bool nullable = false)
        {
            var envVar = Environment.GetEnvironmentVariable(varName);
            if (!nullable && string.IsNullOrEmpty(varName))
            {
                throw new KeyNotFoundException($"{varName} not set.");
            }
            return envVar;
        }
    }
}
