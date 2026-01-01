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

    /// <summary>
    /// Try to get a resource from the cache.
    /// </summary>
    /// <typeparam name="T">The expected type or interface of the resource.</typeparam>
    /// <param name="id">The unique identifier of the resource.</param>
    /// <param name="resource">The resource if found.</param>
    /// <returns>Returns <c>true</c> if the resource was found.</returns>
    bool TryGet<T>(string? id, out T? resource) where T : IResource;

    /// <summary>
    /// Try to get a resource from the cache by type.
    /// </summary>
    /// <typeparam name="T">The expected type or interface of the resource.</typeparam>
    /// <returns>The resources if found.</returns>
    IEnumerable<T> GetType<T>() where T : IResource;
}
