// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule.Pipeline;

/// <summary>
/// A cache that stores resources.
/// </summary>
internal interface IResourceCache : IEnumerable<IResource>
{
    /// <summary>
    /// Import a resource into the cache.
    /// </summary>
    /// <param name="resource">The <see cref="IResource"/> to import.</param>
    /// <returns>Returns <c>true</c> if the resource was imported without issue.</returns>
    bool Import(IResource resource);
}
