// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Host;
using PSRule.Runtime;
using PSRule.Runtime.Scripting;

namespace PSRule.Pipeline;

#nullable enable

/// <summary>
/// Defines a builder to create a resource cache.
/// </summary>
internal sealed class ResourceCacheBuilder(PSRuleOption option, IPipelineWriter? writer, IRunspaceContext? runspaceContext, ILanguageScopeSet languageScopeSet)
{
    private IEnumerable<IResource>? _Resources;
    private readonly IPipelineWriter? _Writer = writer;

    public ResourceCacheBuilder Import(Source[]? sources)
    {
        if (sources == null || sources.Length == 0) return this;

        var context = new ResourceCacheDiscoveryContext(option, _Writer, runspaceContext, languageScopeSet);

        context.Begin();

        try
        {
            _Resources = HostHelper.GetMetaResources<IResource>(sources, context);
            _Resources = HostHelper.GetResources<IResource>(sources, context);
        }
        finally
        {
            context.End();
        }
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
