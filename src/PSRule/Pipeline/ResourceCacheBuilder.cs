// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Host;

namespace PSRule.Pipeline;

#nullable enable

/// <summary>
/// Defines a builder to create a resource cache.
/// </summary>
internal sealed class ResourceCacheBuilder(IPipelineWriter writer)
{
    private IEnumerable<IResource> _Resources;
    private readonly IPipelineWriter _Writer = writer;

    public ResourceCacheBuilder Import(Source[] sources)
    {
        _Resources = HostHelper.GetMetaResources<IResource>(sources, new ResourceCacheDiscoveryContext(_Writer));
        return this;
    }

    public ResourceCache Build(List<ResourceRef>? unresolved)
    {
        var cache = new ResourceCache(unresolved);

        foreach (var resource in _Resources)
            cache.Import(resource);

        return cache;
    }
}

#nullable restore
