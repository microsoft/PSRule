// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// An interface for resource filters.
/// </summary>
public interface IResourceFilter
{
    /// <summary>
    /// The kind of resources the filter applies to.
    /// </summary>
    ResourceKind Kind { get; }

    /// <summary>
    /// Determine if the resource matches the filter.
    /// </summary>
    /// <param name="resource">The resource to match against the filter.</param>
    /// <returns>Returns <c>true</c> if the resource matches the filter; otherwise, <c>false</c>.</returns>
    bool Match(IResource resource);
}
