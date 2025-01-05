// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Host;
using PSRule.Runtime;

namespace PSRule.Pipeline;

#nullable enable

/// <summary>
/// Defines a builder to create a resource cache.
/// </summary>
internal sealed class ResourceCacheBuilder(IPipelineWriter? writer, ILanguageScopeSet languageScopeSet)
{
    private IEnumerable<IResource>? _Resources;
    private readonly IPipelineWriter? _Writer = writer;

    public ResourceCacheBuilder Import(Source[]? sources)
    {
        if (sources == null) return this;

        _Resources = HostHelper.GetMetaResources<IResource>(sources, new ResourceCacheDiscoveryContext(_Writer, languageScopeSet));
        return this;
    }

    public ResourceCache Build(List<ResourceRef>? unresolved)
    {
        var cache = new ResourceCache(unresolved);
        if (_Resources != null)
        {
            // Process module config first.
            foreach (var resource in _Resources.Where(r => r.Kind == ResourceKind.ModuleConfig))
            {
                cache.Import(resource);
            }

            foreach (var resource in _Resources.Where(r => r.Kind != ResourceKind.ModuleConfig))
            {
                cache.Import(resource);
            }
        }
        return cache;
    }
}

#nullable restore
