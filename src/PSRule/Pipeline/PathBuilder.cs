// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Management.Automation;
using PSRule.Data;
using PSRule.Runtime;

namespace PSRule.Pipeline;

internal abstract class PathBuilder(IPipelineWriter logger, string basePath, string searchPattern, IPathFilter? filter, IPathFilter? required)
{
    // Path separators
    private const char Slash = '/';
    private const char BackSlash = '\\';

    private const char Dot = '.';
    private const string CurrentPath = ".";
    private const string RecursiveSearchOperator = "**";

    private static readonly char[] PathLiteralStopCharacters = ['*', '[', '?'];
    private static readonly char[] PathSeparatorCharacters = ['\\', '/'];

    private readonly IPipelineWriter _Logger = logger;
    private readonly List<InputFileInfo> _Files = [];
    private readonly HashSet<string> _Paths = [];
    private readonly string _BasePath = NormalizePath(Environment.GetRootedBasePath(basePath));
    private readonly string _DefaultSearchPattern = searchPattern;
    private readonly IPathFilter? _GlobalFilter = filter;
    private readonly IPathFilter? _Required = required;

    /// <summary>
    /// The number of files found.
    /// </summary>
    public int Count => _Files.Count;

    /// <summary>
    /// Add an array of paths to the builder.
    /// </summary>
    public void Add(string[] path)
    {
        if (path == null || path.Length == 0)
            return;

        for (var i = 0; i < path.Length; i++)
            Add(path[i]);
    }

    /// <summary>
    /// Add a path to the builder.
    /// </summary>
    /// <param name="path">The path to add.</param>
    /// <param name="useGlobalFilter">When <c>true</c> the global filter will be used to limit the included paths.</param>
    public void Add(string path, bool useGlobalFilter = true)
    {
        if (string.IsNullOrEmpty(path))
            return;

        try
        {
            FindFiles(path, useGlobalFilter);
        }
        catch (Exception ex)
        {
            _Logger.ErrorReadInputFailed(path, ex);
        }
    }

    public InputFileInfo[] Build()
    {
        LogFilesDiagnostic(_Files);
        return [.. _Files];
    }

    private void LogFilesDiagnostic(List<InputFileInfo> files)
    {
        if (_Logger == null || files.Count == 0) return;

        foreach (var file in _Files)
        {
            _Logger.LogDebug(EventId.None, "Included file path: {0}", file.Path);
        }
    }

    private void FindFiles(string path, bool useGlobalFilter)
    {
        if (TryUrl(path) || TryLiteralPath(path, out path))
            return;

        var pathLiteral = GetSearchParameters(path, out var searchPattern, out var searchOption, out var filter);
        var files = Directory.EnumerateFiles(pathLiteral, searchPattern, searchOption);

        foreach (var file in files)
        {
            if (ShouldInclude(file, filter, useGlobalFilter))
            {
                AddFile(file);
            }
        }
    }

    private bool TryUrl(string path)
    {
        if (!path.IsURL())
            return false;

        AddFile(path);
        return true;
    }

    /// <summary>
    /// Determine if the specified path is a specific file or directory.
    /// If the path is an existing file, add it to the result set.
    /// If the path is an existing directory, normalize the path and return false.
    /// </summary>
    private bool TryLiteralPath(string path, out string normalPath)
    {
        normalPath = path;
        if (path.IndexOfAny(PathLiteralStopCharacters) > -1)
            return false;

        var rootedPath = GetRootedPath(path);
        if (Directory.Exists(rootedPath) || path == CurrentPath)
        {
            normalPath = IsBasePath(rootedPath) ? CurrentPath : NormalizeDirectoryPath(path);
            return false;
        }
        if (!File.Exists(rootedPath))
        {
            ErrorNotFound(path);
            return false;
        }
        AddFile(rootedPath);
        return true;
    }

    private bool IsBasePath(string path)
    {
        return NormalizeDirectoryPath(path) == _BasePath;
    }

    private void ErrorNotFound(string path)
    {
        if (_Logger == null)
            return;

        _Logger.LogError(new ErrorRecord(new FileNotFoundException(), "PSRule.PathBuilder.ErrorNotFound", ErrorCategory.ObjectNotFound, path));
    }

    /// <summary>
    /// Add a file to the result set that will be returned.
    /// </summary>
    private void AddFile(string path)
    {
        if (_Paths.Contains(path))
            return;

        _Files.Add(new InputFileInfo(_BasePath, path));
        _Paths.Add(path);
    }

    private string GetRootedPath(string path)
    {
        return Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(_BasePath, path));
    }

    /// <summary>
    /// Split a search path into components based on wildcards.
    /// </summary>
    private string GetSearchParameters(string path, out string searchPattern, out SearchOption searchOption, out PathFilter filter)
    {
        searchOption = SearchOption.AllDirectories;
        var pathLiteral = TrimPath(path, out var relativeAnchor);

        if (TryFilter(pathLiteral, out searchPattern, out filter))
            return _BasePath;

        pathLiteral = SplitSearchPath(pathLiteral, out searchPattern);
        if ((relativeAnchor || !string.IsNullOrEmpty(pathLiteral)) && !string.IsNullOrEmpty(searchPattern))
            searchOption = SearchOption.TopDirectoryOnly;

        if (string.IsNullOrEmpty(searchPattern))
            searchPattern = _DefaultSearchPattern;

        return GetRootedPath(pathLiteral);
    }

    private static string SplitSearchPath(string path, out string searchPattern)
    {
        // Find the index of the first expression character i.e. out/modules/**/file
        var stopIndex = path.IndexOfAny(PathLiteralStopCharacters);

        // Track back to the separator before any expression characters
        var literalSeparator = stopIndex > -1 ? path.LastIndexOfAny(PathSeparatorCharacters, stopIndex) + 1 : path.LastIndexOfAny(PathSeparatorCharacters) + 1;
        searchPattern = path.Substring(literalSeparator, path.Length - literalSeparator);
        return path.Substring(0, literalSeparator);
    }

    private bool TryFilter(string path, out string? searchPattern, out PathFilter? filter)
    {
        searchPattern = null;
        filter = null;
        if (UseSimpleSearch(path))
            return false;

        filter = PathFilter.Create(_BasePath, path);
        var patternSeparator = path.LastIndexOfAny(PathSeparatorCharacters) + 1;
        searchPattern = path.Substring(patternSeparator, path.Length - patternSeparator);
        return true;
    }

    /// <summary>
    /// Remove leading ./ or .\ characters indicating a relative path anchor.
    /// </summary>
    /// <param name="path">The path to trim.</param>
    /// <param name="relativeAnchor">Returns true when a relative path anchor was present.</param>
    /// <returns>Return a clean path without the relative path anchor.</returns>
    private static string TrimPath(string path, out bool relativeAnchor)
    {
        relativeAnchor = false;
        if (path.Length >= 2 && path[0] == Dot && IsSeparator(path[1]))
        {
            relativeAnchor = true;
            return path.Remove(0, 2);
        }
        return path;
    }

    private bool ShouldInclude(string file, PathFilter filter, bool useGlobalFilter)
    {
        return (filter == null || filter.Match(file)) &&
            (_Required == null || _Required.Match(file)) &&
            (_GlobalFilter == null || useGlobalFilter == false || _GlobalFilter.Match(file));
    }

    [DebuggerStepThrough]
    private static bool IsSeparator(char c)
    {
        return c == Slash || c == BackSlash;
    }

    /// <summary>
    /// Determines if a simple search can be used.
    /// </summary>
    [DebuggerStepThrough]
    private static bool UseSimpleSearch(string s)
    {
        return s.IndexOf(RecursiveSearchOperator, StringComparison.OrdinalIgnoreCase) == -1;
    }

    [DebuggerStepThrough]
    private static string NormalizePath(string path)
    {
        return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
    }

    [DebuggerStepThrough]
    private static string NormalizeDirectoryPath(string path)
    {
        return NormalizePath(
            IsSeparator(path[path.Length - 1]) ? path : string.Concat(path, Path.DirectorySeparatorChar)
        );
    }
}
