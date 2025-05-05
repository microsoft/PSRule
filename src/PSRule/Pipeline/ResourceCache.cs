// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using PSRule.Definitions;
using PSRule.Definitions.Baselines;
using PSRule.Definitions.ModuleConfigs;
using PSRule.Definitions.Selectors;
using PSRule.Definitions.SuppressionGroups;

namespace PSRule.Pipeline;

#nullable enable

/// <summary>
/// Define a cache for resources.
/// </summary>
internal sealed class ResourceCache(IList<ResourceRef>? unresolved) : IResourceCache
{
    /// <summary>
    /// Track a list of resource references that should be resolved once all resources are imported.
    /// </summary>
    private readonly IList<ResourceRef> _Unresolved = unresolved ?? [];

    /// <summary>
    /// Track a list of issues that should be reported once all resources are imported.
    /// </summary>
    private readonly List<ResourceIssue> _TrackedIssues = [];

    /// <summary>
    /// A list of resources.
    /// </summary>
    private readonly List<IResource> _Resources = [];

    internal readonly Dictionary<string, (Baseline baseline, BaselineRef baselineRef)> Baselines = new(StringComparer.OrdinalIgnoreCase);

    public IEnumerable<ResourceIssue> Issues => _TrackedIssues;

    public IEnumerable<ResourceRef> Unresolved => _Unresolved;

    /// <inheritdoc/>
    public bool Import(IResource resource)
    {
        if (resource == null) throw new ArgumentNullException(nameof(resource));

        if (TrackIssue(resource))
        {

        }
        else if (TryBaseline(resource, out var baseline) && TryBaselineRef(resource.Id, out var baselineRef))
        {
            RemoveBaselineRef(resource.Id);
            _Resources.Add(baseline!);
            Baselines.Add(resource.Id.Value, (baseline!, baselineRef!));
            return true;
        }
        else if (TrySelector(resource, out var selector))
        {
            _Resources.Add(selector!);
            return true;
        }
        else if (TryModuleConfig(resource, out var moduleConfig))
        {
            switch (moduleConfig!.Spec)
            {
                case IModuleConfigV1Spec v1:
                    TrackUnresolvedBaseline(moduleConfig.Source.Module, v1.Rule?.Baseline);
                    break;

                case IModuleConfigV2Spec v2:
                    TrackUnresolvedBaseline(moduleConfig.Source.Module, v2.Rule?.Baseline);
                    break;
            }

            _Resources.Add(moduleConfig);
            return true;
        }
        else if (TrySuppressionGroup(resource, out var suppressionGroup))
        {
            _Resources.Add(suppressionGroup!);
            return true;
        }
        else if (resource.Kind == ResourceKind.Rule)
        {
            _Resources.Add(resource);
            return true;
        }
        else if (resource.Kind == ResourceKind.Convention)
        {
            _Resources.Add(resource);
            return true;
        }

        return false;
    }

    private void TrackUnresolvedBaseline(string? module, string? baseline)
    {
        if (!string.IsNullOrEmpty(baseline) && baseline != null && module != null)
        {
            var baselineId = ResourceHelper.GetIdString(module, baseline);
            if (!Baselines.ContainsKey(baselineId))
            {
                _Unresolved.Add(new BaselineRef(baselineId, ScopeType.Baseline));
            }
        }
    }

    /// <summary>
    /// Check for and track resource issues.
    /// </summary>
    /// <returns>If the resource should be ignored then return <c>true</c>, otherwise <c>false</c> is returned.</returns>
    private bool TrackIssue(IResource resource)
    {
        if (TrySuppressionGroup(resource, out var suppressionGroup) && suppressionGroup != null)
        {
            switch (suppressionGroup.Spec)
            {
                case ISuppressionGroupV1Spec v1:
                    if (v1.ExpiresOn.HasValue && v1.ExpiresOn.Value <= DateTime.UtcNow)
                    {
                        _TrackedIssues.Add(new ResourceIssue(ResourceIssueType.SuppressionGroupExpired, resource.Id));
                        return true;
                    }
                    break;

                case ISuppressionGroupV2Spec v2:
                    if (v2.ExpiresOn.HasValue && v2.ExpiresOn.Value <= DateTime.UtcNow)
                    {
                        _TrackedIssues.Add(new ResourceIssue(ResourceIssueType.SuppressionGroupExpired, resource.Id));
                        return true;
                    }
                    break;
            }
        }
        return false;
    }

    private bool TryBaselineRef(ResourceId resourceId, out BaselineRef? baselineRef)
    {
        baselineRef = null;
        var r = _Unresolved.FirstOrDefault(i => ResourceIdEqualityComparer.IdEquals(i.Id, resourceId.Value));
        if (r is not BaselineRef br)
            return false;

        baselineRef = br;
        return true;
    }

    private void RemoveBaselineRef(ResourceId resourceId)
    {
        foreach (var r in _Unresolved.ToArray())
        {
            if (ResourceIdEqualityComparer.IdEquals(r.Id, resourceId.Value))
                _Unresolved.Remove(r);
        }
    }

    private static bool TryBaseline(IResource resource, out Baseline? baseline)
    {
        baseline = null;
        if (resource.Kind == ResourceKind.Baseline && resource is Baseline result)
        {
            baseline = result;
            return true;
        }
        return false;
    }

    private static bool TryModuleConfig(IResource resource, out IModuleConfig? moduleConfig)
    {
        moduleConfig = null;
        if (resource.Kind == ResourceKind.ModuleConfig && resource is IModuleConfig result)
        {
            moduleConfig = result;
            return true;
        }
        return false;
    }

    private static bool TrySelector(IResource resource, out ISelector? selector)
    {
        selector = null;
        if (resource.Kind == ResourceKind.Selector && resource is ISelector result)
        {
            selector = result;
            return true;
        }
        return false;
    }

    private static bool TrySuppressionGroup(IResource resource, out ISuppressionGroup? suppressionGroup)
    {
        suppressionGroup = null;
        if (resource.Kind == ResourceKind.SuppressionGroup && resource is ISuppressionGroup result)
        {
            suppressionGroup = result;
            return true;
        }
        return false;
    }

    #region IEnumerable<IResource>

    public IEnumerator<IResource> GetEnumerator()
    {
        return _Resources.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion IEnumerable<IResource>
}

#nullable restore
