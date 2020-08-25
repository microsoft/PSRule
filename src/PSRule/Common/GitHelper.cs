// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;

namespace PSRule
{
    internal static class GitHelper
    {
        public static string GetHeadRef(string path)
        {
            // Try PSRule
            if (EnvironmentHelper.TryString("PSRULE_GITREF", out string value))
                return value;

            // Try Azure Pipelines
            if (EnvironmentHelper.TryString("BUILD_SOURCEBRANCH", out value))
                return value;

            // Try GitHub Actions
            if (EnvironmentHelper.TryString("GITHUB_REF", out value))
                return value;

            // Try .git/HEAD
            if (TryReadHead(path, out value))
                return value;

            return null;
        }

        private static bool TryReadHead(string path, out string value)
        {
            value = null;
            var headFilePath = Path.Combine(path, "HEAD");
            if (!File.Exists(headFilePath))
                return false;

            value = File.ReadAllText(headFilePath);
            if (value.StartsWith("ref: ", System.StringComparison.OrdinalIgnoreCase))
                value = value.Substring(5);

            return true;
        }
    }
}
