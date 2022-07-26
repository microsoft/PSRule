// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using PSRule.Configuration;

namespace PSRule
{
    /// <summary>
    /// Helper for working with Git and CI tools.
    /// </summary>
    /// <remarks>
    /// Azure Pipelines: https://docs.microsoft.com/azure/devops/pipelines/build/variables
    /// GitHub Actions: https://docs.github.com/actions/learn-github-actions/environment-variables#default-environment-variables
    /// </remarks>
    internal static class GitHelper
    {
        private const string GIT_HEAD = "HEAD";
        private const string GIT_REF_PREFIX = "ref: ";
        private const string GIT_DEFAULT_PATH = ".git/";
        private const string GIT_REF_HEAD = "refs/heads/";

        private const string GITHUB_BASE_URL = "https://github.com/";

        // Environment variables for PSRule
        private const string ENV_PSRULE_REPO_REF = "PSRULE_REPOSITORY_REF";
        private const string ENV_PSRULE_REPO_HEADREF = "PSRULE_REPOSITORY_HEADREF";
        private const string ENV_PSRULE_REPO_BASEREF = "PSRULE_REPOSITORY_BASEREF";
        private const string ENV_PSRULE_REPO_REVISION = "PSRULE_REPOSITORY_REVISION";
        private const string ENV_PSRULE_REPO_URL = "PSRULE_REPOSITORY_URL";

        // Environment variables for Azure Pipelines
        private const string ENV_ADO_REPO_REF = "BUILD_SOURCEBRANCH";
        private const string ENV_ADO_REPO_BASEREF = "SYSTEM_PULLREQUEST_TARGETBRANCH";
        private const string ENV_ADO_REPO_REVISION = "BUILD_SOURCEVERSION";
        private const string ENV_ADO_REPO_URL = "BUILD_REPOSITORY_URI";

        // Environment variables for GitHub Actions
        private const string ENV_GITHUB_REPO_REF = "GITHUB_REF";
        private const string ENV_GITHUB_REPO_HEADREF = "GITHUB_HEAD_REF";
        private const string ENV_GITHUB_REPO_BASEREF = "GITHUB_BASE_REF";
        private const string ENV_GITHUB_REPO_REVISION = "GITHUB_SHA";
        private const string ENV_GITHUB_REPO_URL = "GITHUB_REPOSITORY";

        /// <summary>
        /// The get HEAD ref.
        /// </summary>
        public static bool TryHeadRef(out string value, string path = null)
        {
            // Try PSRule
            if (EnvironmentHelper.Default.TryString(ENV_PSRULE_REPO_REF, out value) ||
                EnvironmentHelper.Default.TryString(ENV_PSRULE_REPO_HEADREF, out value))
                return true;

            // Try Azure Pipelines
            if (EnvironmentHelper.Default.TryString(ENV_ADO_REPO_REF, out value))
                return true;

            // Try GitHub Actions
            if (EnvironmentHelper.Default.TryString(ENV_GITHUB_REPO_REF, out value) ||
                EnvironmentHelper.Default.TryString(ENV_GITHUB_REPO_HEADREF, out value))
                return true;

            // Try .git/
            return TryReadHead(path, out value);
        }

        /// <summary>
        /// Get the HEAD branch name.
        /// </summary>
        public static bool TryHeadBranch(out string value, string path = null)
        {
            value = TryHeadRef(out value, path) && value.StartsWith(GIT_REF_HEAD) ? value.Substring(11) : value;
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Get the target ref.
        /// </summary>
        public static bool TryBaseRef(out string value, string path = null)
        {
            // Try PSRule
            if (EnvironmentHelper.Default.TryString(ENV_PSRULE_REPO_BASEREF, out value))
                return true;

            // Try Azure Pipelines
            if (EnvironmentHelper.Default.TryString(ENV_ADO_REPO_BASEREF, out value))
                return true;

            // Try GitHub Actions
            if (EnvironmentHelper.Default.TryString(ENV_GITHUB_REPO_BASEREF, out value))
                return true;

            // Try .git/
            return TryReadHead(path, out value);
        }

        public static bool TryRevision(out string value, string path = null)
        {
            // Try PSRule
            if (EnvironmentHelper.Default.TryString(ENV_PSRULE_REPO_REVISION, out value))
                return true;

            // Try Azure Pipelines
            if (EnvironmentHelper.Default.TryString(ENV_ADO_REPO_REVISION, out value))
                return true;

            // Try GitHub Actions
            if (EnvironmentHelper.Default.TryString(ENV_GITHUB_REPO_REVISION, out value))
                return true;

            // Try .git/
            return TryReadCommit(path, out value);
        }

        public static bool TryRepository(out string value, string path = null)
        {
            // Try PSRule
            if (EnvironmentHelper.Default.TryString(ENV_PSRULE_REPO_URL, out value))
                return true;

            // Try Azure Pipelines
            if (EnvironmentHelper.Default.TryString(ENV_ADO_REPO_URL, out value))
                return true;

            // Try GitHub Actions
            if (EnvironmentHelper.Default.TryString(ENV_GITHUB_REPO_URL, out value))
            {
                value = string.Concat(GITHUB_BASE_URL, value);
                return true;
            }

            // Try .git/
            return false;
        }

        private static bool TryReadHead(string path, out string value)
        {
            value = null;
            return TryGitFile(path, GIT_HEAD, out var filePath) && TryCommit(filePath, out value, out _);
        }

        private static bool TryReadCommit(string path, out string value)
        {
            value = null;
            if (!TryGitFile(path, GIT_HEAD, out var filePath))
                return false;

            while (TryCommit(filePath, out value, out var isRef) && isRef)
                TryGitFile(path, value, out filePath);

            return value != null;
        }

        private static bool TryGitFile(string path, string file, out string filePath)
        {
            path ??= PSRuleOption.GetRootedBasePath(GIT_DEFAULT_PATH);
            filePath = Path.Combine(path, file);
            return File.Exists(filePath);
        }

        private static bool TryCommit(string path, out string value, out bool isRef)
        {
            value = null;
            isRef = false;
            var lines = File.ReadAllLines(path);
            if (lines == null || lines.Length == 0)
                return false;

            isRef = lines[0].StartsWith(GIT_REF_PREFIX, System.StringComparison.OrdinalIgnoreCase);
            value = isRef ? lines[0].Substring(5) : lines[0];
            return true;
        }
    }
}
