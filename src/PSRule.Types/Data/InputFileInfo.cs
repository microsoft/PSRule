// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Expressions;

namespace PSRule.Data;

/// <summary>
/// An input file information structure.
/// </summary>
public sealed class InputFileInfo : ITargetInfo
{
    private readonly string? _TargetType;
    private readonly TargetSourceInfo _Source;

    internal readonly bool IsUrl;

    internal InputFileInfo(string basePath, string path)
    {
        FullName = path;
        _Source = new TargetSourceInfo(this);
        if (path.IsURL())
        {
            IsUrl = true;
            return;
        }
        BasePath = basePath;
        Name = System.IO.Path.GetFileName(path);
        Extension = System.IO.Path.GetExtension(path);
        DirectoryName = System.IO.Path.GetDirectoryName(Environment.GetRootedPath(path, normalize: false, basePath: basePath));
        DisplayName = PathHelpers.NormalizePath(basePath, FullName);
        Path = PathHelpers.NormalizePath(basePath, FullName);
        _TargetType = string.IsNullOrEmpty(Extension) ? System.IO.Path.GetFileNameWithoutExtension(path) : Extension;
    }

    /// <summary>
    /// The fully qualified path to the file.
    /// </summary>
    public string? FullName { get; }

    /// <summary>
    /// The base path containing the file.
    /// </summary>
    public string? BasePath { get; }

    /// <summary>
    /// The parent path to the file.
    /// </summary>
    public string? ParentPath { get; }

    /// <summary>
    /// The file name.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// The file extension.
    /// </summary>
    public string? Extension { get; }

    /// <summary>
    /// The name of the directory containing the file.
    /// </summary>
    public string? DirectoryName { get; }

    /// <summary>
    /// The normalized path to the file.
    /// </summary>
    public string? Path { get; }

    /// <summary>
    /// The friendly display name for the file.
    /// </summary>
    public string? DisplayName { get; }

    string? ITargetInfo.Name => DisplayName;

    string? ITargetInfo.Type => _TargetType;

    TargetSourceInfo ITargetInfo.Source => _Source;

    /// <summary>
    /// Convert to string.
    /// </summary>
    public override string? ToString()
    {
        return FullName;
    }

    /// <summary>
    /// Convert to FileInfo.
    /// </summary>
    public FileInfo AsFileInfo()
    {
        return new FileInfo(FullName);
    }
}
