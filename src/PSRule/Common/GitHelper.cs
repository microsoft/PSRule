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
            if (EnvironmentHelper.Default.TryString("PSRULE_GITREF", out var value))
                return value;

            // Try Azure Pipelines
            if (EnvironmentHelper.Default.TryString("BUILD_SOURCEBRANCH", out value))
                return value;

            // Try GitHub Actions
            if (EnvironmentHelper.Default.TryString("GITHUB_REF", out value))
                return value;

            // Try .git/HEAD
            return TryReadHead(path, out value) ? value : null;
        }

        private static bool TryReadHead(string path, out string value)
        {
            value = null;
            var headFilePath = Path.Combine(path, "HEAD");
            if (!File.Exists(headFilePath))
                return false;

            var lines = File.ReadAllLines(headFilePath);
            if (lines == null || lines.Length == 0)
                return false;

            value = lines[0].StartsWith("ref: ", System.StringComparison.OrdinalIgnoreCase) ? lines[0].Substring(5) : lines[0];

            return true;
        }
    }
}
