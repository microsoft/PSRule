// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// Metadata about a PSRule resource.
/// </summary>
public interface IResourceHelpInfo
{
    /// <summary>
    /// The name of the resource.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// A display name of the resource if set.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// A short description of the resource if set.
    /// </summary>
    InfoString Synopsis { get; }

    /// <summary>
    /// A long description of the resource if set.
    /// </summary>
    InfoString Description { get; }
}
