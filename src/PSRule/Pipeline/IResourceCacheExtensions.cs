// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule.Pipeline;

#nullable enable

/// <summary>
/// Extension methods for <see cref="IResourceCache"/>.
/// </summary>
internal static class IResourceCacheExtensions
{
    /// <summary>
    /// Import a collection of resources into a cache.
    /// </summary>
    /// <param name="cache">The <see cref="IResourceCache"/> instance to import into.</param>
    /// <param name="resource">The resources to import into the <see cref="IResourceCache"/> instance.</param>
    /// <typeparam name="TResource">The type of resource to import.</typeparam>
    public static void Import<TResource>(this IResourceCache cache, IEnumerable<TResource>? resource)
        where TResource : IResource
    {
        if (cache == null || resource == null) return;

        foreach (var r in resource)
        {
            cache.Import(r);
        }
    }
}

#nullable restore
