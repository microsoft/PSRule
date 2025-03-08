// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Pipeline;

#nullable enable

/// <summary>
/// Wrap a path filter with a filter that only allows files that have changed.
/// </summary>
internal sealed class ChangedFilesPathFilter : IPathFilter
{
    private readonly IPathFilter _Inner;
    private readonly string _BasePath;
    private readonly HashSet<string>? _ChangedFiles;
    private readonly StringComparison _FilePathComparer = StringComparison.Ordinal;

    public ChangedFilesPathFilter(IPathFilter inner, string basePath, string[] changedFiles)
    {
        if (string.IsNullOrEmpty(basePath)) throw new ArgumentNullException(nameof(basePath));
        if (changedFiles == null) throw new ArgumentNullException(nameof(changedFiles));

        _Inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _BasePath = Environment.GetRootedBasePath(basePath, normalize: true);
        _ChangedFiles = changedFiles.Length == 0 ? null : new HashSet<string>(changedFiles, _FilePathComparer == StringComparison.Ordinal ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public bool Match(string path)
    {
        if (_ChangedFiles == null || string.IsNullOrEmpty(path))
            return false;

        // Check if the path is rooted and starts with the base path.
        var rootedFilePath = Environment.GetRootedPath(path, normalize: true, basePath: _BasePath);
        if (!rootedFilePath.StartsWith(_BasePath, _FilePathComparer))
            return false;

        var relativePath = rootedFilePath.Substring(_BasePath.Length);
        return _ChangedFiles.Contains(relativePath) && _Inner.Match(path);
    }
}

#nullable restore
