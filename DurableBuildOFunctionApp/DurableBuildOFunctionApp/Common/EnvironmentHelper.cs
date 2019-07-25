using System;
using System.Collections;
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

        public static string GetProject(string worker = null)
        {
            if (!string.IsNullOrEmpty(worker) && worker == "python")
            {
                return GetEnvironment(Constants.EnvVars.DevOpsProjectPython);
            }
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

        public static bool ShouldUploadArtifact()
        {
            var shouldUpload = GetEnvironment(Constants.EnvVars.UploadArtifact);
            if (string.IsNullOrEmpty(shouldUpload))
            {
                return false;
            }
            return shouldUpload.Equals("True", StringComparison.OrdinalIgnoreCase)
                || shouldUpload.Equals("1", StringComparison.OrdinalIgnoreCase);
        }

        public static string GetStorageConnectionString()
        {
            return GetEnvironment(Constants.EnvVars.UploadStorageConString);
        }

        public static string GetHostEnvironment()
        {
            return GetEnvironment(Constants.EnvVars.HostEnvironment, nullable: true)
                ?? "dev";
        }

        public static string GetArtifactName(string prefix, string commonPrefix = null)
        {
            var prefixToUse = prefix ?? commonPrefix;
            if (string.IsNullOrEmpty(prefixToUse))
            {
                throw new ArgumentNullException($"Both {nameof(prefix)} and {nameof(commonPrefix)} cannot be null.");
            }
            return GetEnvironment($"{prefixToUse}{Constants.EnvVars.ArtifactNameSuffix}");
        }

        public static IDictionary<string, string> GetPrefixVariables(string prefix)
        {
            IDictionary<string, string> variables = new Dictionary<string, string>();
            foreach (DictionaryEntry env in Environment.GetEnvironmentVariables())
            {
                // If settings start with this, we strip the prefix out and record the value
                if (env.Key.ToString()?.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                    ?? false)
                {
                    variables[env.Key.ToString().Substring(prefix.Length)] = env.Value.ToString();
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
