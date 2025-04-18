// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.InteropServices;

namespace PSRule;

/// <summary>
/// Helper for working with Git and CI tools.
/// </summary>
/// <remarks>
/// Docs for <seealso href="https://docs.microsoft.com/azure/devops/pipelines/build/variables">Azure Pipelines</seealso> and
/// <seealso href="https://docs.github.com/actions/learn-github-actions/environment-variables#default-environment-variables">GitHub Actions</seealso>.
/// </remarks>
internal static class GitHelper
{
    private const string GIT_HEAD = "HEAD";
    private const string GIT_REF_PREFIX = "ref: ";
    private const string GIT_GITDIR_PREFIX = "gitdir: ";
    private const string GIT_DEFAULT_PATH = ".git";
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
    public static bool TryHeadRef(out string value)
    {
        // Try PSRule
        if (Environment.TryString(ENV_PSRULE_REPO_REF, out value) ||
            Environment.TryString(ENV_PSRULE_REPO_HEADREF, out value))
            return true;

        // Try Azure Pipelines
        if (Environment.TryString(ENV_ADO_REPO_REF, out value))
            return true;

        // Try GitHub Actions
        if (Environment.TryString(ENV_GITHUB_REPO_REF, out value) ||
            Environment.TryString(ENV_GITHUB_REPO_HEADREF, out value))
            return true;

        // Try .git/
        return TryReadHead(out value);
    }

    /// <summary>
    /// Get the HEAD branch name.
    /// </summary>
    public static bool TryHeadBranch(out string value)
    {
        value = TryHeadRef(out value) && value.StartsWith(GIT_REF_HEAD) ? value.Substring(11) : value;
        return !string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Get the target ref.
    /// </summary>
    public static bool TryBaseRef(out string value)
    {
        // Try PSRule
        if (Environment.TryString(ENV_PSRULE_REPO_BASEREF, out value))
            return true;

        // Try Azure Pipelines
        if (Environment.TryString(ENV_ADO_REPO_BASEREF, out value))
            return true;

        // Try GitHub Actions
        if (Environment.TryString(ENV_GITHUB_REPO_BASEREF, out value))
            return true;

        // Try .git/
        return TryReadHead(out value);
    }

    public static bool TryRevision(out string value)
    {
        // Try PSRule
        if (Environment.TryString(ENV_PSRULE_REPO_REVISION, out value))
            return true;

        // Try Azure Pipelines
        if (Environment.TryString(ENV_ADO_REPO_REVISION, out value))
            return true;

        // Try GitHub Actions
        if (Environment.TryString(ENV_GITHUB_REPO_REVISION, out value))
            return true;

        // Try .git/
        return TryReadCommit(out value);
    }

    public static bool TryRepository(out string value, string path = null)
    {
        // Try PSRule
        if (Environment.TryString(ENV_PSRULE_REPO_URL, out value))
            return true;

        // Try Azure Pipelines
        if (Environment.TryString(ENV_ADO_REPO_URL, out value))
            return true;

        // Try GitHub Actions
        if (Environment.TryString(ENV_GITHUB_REPO_URL, out value))
        {
            value = string.Concat(GITHUB_BASE_URL, value);
            return true;
        }

        // Try .git/
        return TryGetOriginUrl(path, out value);
    }

    public static bool TryGetChangedFiles(string baseRef, string filter, string? options, out string[] files)
    {
        // Get current tip
        var source = TryRevision(out var source_sha) ? source_sha : GIT_HEAD;
        var target = !string.IsNullOrEmpty(baseRef) ? baseRef : "HEAD^";

        var bin = GetGitBinary();
        var args = GetDiffArgs(target, source, filter, options);
        var tool = ExternalTool.Get(null, bin);

        files = [];
        if (tool == null || !tool.WaitForExit(args, out var exitCode) || exitCode != 0)
            return false;

        files = tool.GetOutput().Split([System.Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
        return true;
    }

    #region Helper methods

    internal static bool TryReadHead(out string value, string? path = null)
    {
        value = null;
        return TryGitFile(GIT_HEAD, out var filePath, path) && TryReadRef(filePath, out value, out _);
    }

    private static string GetGitBinary()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "git" : "git.exe";
    }

    private static string GetDiffArgs(string target, string source, string filter, string? options)
    {
        return $"diff --diff-filter={filter} --ignore-submodules=all --name-only --no-renames {target}";
    }

    private static bool TryReadCommit(out string value)
    {
        value = null;
        if (!TryGitFile(GIT_HEAD, out var filePath))
            return false;

        while (TryReadRef(filePath, out value, out var isRef) && isRef)
            TryGitFile(value, out filePath);

        return value != null;
    }

    private static bool TryGitFile(string file, out string filePath, string path = null)
    {
        var gitPath = GetGitDir(path);
        filePath = Path.Combine(gitPath, file);
        return File.Exists(filePath);
    }

    private static string GetGitDir(string path = null)
    {
        path = Environment.GetRootedPath(GIT_DEFAULT_PATH, basePath: Environment.GetRootedPath(path));

        // Try the case of a submodule.
        if (File.Exists(path) && TryReadGitDirEntry(path, out var gitDir))
            return gitDir;

        // Try the simple case of .git/.
        return path;
    }

    private static bool TryReadGitDirEntry(string filePath, out string value)
    {
        value = null;
        if (!TryReadFirstLineFromGitFile(filePath, out var line))
            return false;

        if (!line.StartsWith(GIT_GITDIR_PREFIX, StringComparison.OrdinalIgnoreCase))
            return false;

        value = Environment.GetRootedBasePath(line.Substring(8), basePath: Path.GetDirectoryName(filePath));
        return true;
    }

    private static bool TryReadRef(string path, out string value, out bool isRef)
    {
        value = null;
        isRef = false;
        if (!TryReadFirstLineFromGitFile(path, out var line))
            return false;

        isRef = line.StartsWith(GIT_REF_PREFIX, StringComparison.OrdinalIgnoreCase);
        value = isRef ? line.Substring(5) : line;
        return true;
    }

    /// <summary>
    /// Read the first line of a git file.
    /// </summary>
    private static bool TryReadFirstLineFromGitFile(string path, out string value)
    {
        value = null;
        if (!File.Exists(path))
            return false;

        var lines = File.ReadAllLines(path);
        if (lines == null || lines.Length == 0)
            return false;

        value = lines[0];
        return true;
    }

    /// <summary>
    /// Try to get the origin URL from the git config.
    /// </summary>
    private static bool TryGetOriginUrl(string path, out string value)
    {
        value = null;

        try
        {
            var bin = GetGitBinary();
            var args = GetWorktreeConfigArgs();
            var tool = ExternalTool.Get(null, bin);

            string[] lines = null;
            if (!tool.WaitForExit(args, out var exitCode) || exitCode != 0)
                return false;

            lines = tool.GetOutput().Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var origin = lines.Where(line => line.StartsWith("remote.origin.url=", StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.Split('=')?[1];
            value = origin;
        }
        catch
        {
            // Fail silently.
        }

        return value != null;
    }

    private static string GetWorktreeConfigArgs()
    {
        return "config --worktree --list";
    }

    #endregion Helper methods
}
