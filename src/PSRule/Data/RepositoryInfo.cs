// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Data;

/// <summary>
/// Repository target information.
/// </summary>
public sealed class RepositoryInfo : ITargetInfo
{
    internal RepositoryInfo(string basePath, string headRef)
    {
        FullName = basePath;
        BasePath = basePath;
        DisplayName = headRef;
    }

    /// <summary>
    /// The full path to the repository root.
    /// </summary>
    public string FullName { get; }

    /// <summary>
    /// The full path to the repository root.
    /// </summary>
    public string BasePath { get; }

    /// <summary>
    /// The HEAD ref.
    /// </summary>
    public string DisplayName { get; }

    /// <inheritdoc/>
    string ITargetInfo.TargetName => DisplayName;

    /// <inheritdoc/>
    string ITargetInfo.TargetType => typeof(RepositoryInfo).FullName;

    /// <inheritdoc/>
    TargetSourceInfo ITargetInfo.Source => null;
}
