// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Pipeline;

/// <summary>
/// Build a path filter from standard inputs.
/// </summary>
internal sealed class PathFilterBuilder
{
    private const string GitIgnoreFileName = ".gitignore";

    private static readonly string[] CommonFiles =
    [
        "README.md",
        ".DS_Store",
        ".gitignore",
        ".gitattributes",
        ".gitmodules",
        "LICENSE",
        "LICENSE.txt",
        "CODE_OF_CONDUCT.md",
        "CONTRIBUTING.md",
        "SECURITY.md",
        "SUPPORT.md",
        ".vscode/*.json",
        ".vscode/*.code-snippets",
        ".github/**/*.md",
        ".github/CODEOWNERS",
        ".pipelines/**/*.yml",
        ".pipelines/**/*.yaml",
        ".azure-pipelines/**/*.yml",
        ".azure-pipelines/**/*.yaml",
        ".azuredevops/*.md"
    ];

    private readonly string _BasePath;
    private readonly List<string> _Expressions;
    private readonly bool _MatchResult;

    private PathFilterBuilder(string basePath, string[]? expressions, bool matchResult, bool ignoreGitPath, bool ignoreRepositoryCommon)
    {
        _BasePath = basePath;
        _Expressions = expressions == null || expressions.Length == 0 ? [] : [.. expressions];
        _MatchResult = matchResult;
        if (ignoreRepositoryCommon)
            _Expressions.InsertRange(0, CommonFiles);

        if (ignoreGitPath)
            _Expressions.Add(".git/");
    }

    internal static PathFilterBuilder Create(string basePath, string[]? expressions, bool ignoreGitPath, bool ignoreRepositoryCommon)
    {
        return new PathFilterBuilder(basePath, expressions, false, ignoreGitPath, ignoreRepositoryCommon);
    }

    /// <summary>
    /// Import expressions from .gitignore.
    /// </summary>
    internal void UseGitIgnore(string? basePath = null)
    {
        ReadFile(Path.Combine(basePath ?? _BasePath, GitIgnoreFileName));
    }

    internal PathFilter Build()
    {
        return PathFilter.Create(_BasePath, [.. _Expressions], _MatchResult);
    }

    private void ReadFile(string filePath)
    {
        if (File.Exists(filePath))
            _Expressions.AddRange(File.ReadAllLines(filePath));
    }
}
