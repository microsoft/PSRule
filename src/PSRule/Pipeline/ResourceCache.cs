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
internal sealed class ResourceCache : IResourceCache
{
    private readonly List<ResourceIssue> _TrackedIssues;
    private readonly IList<ResourceRef> _Unresolved;
    private readonly List<IResource> _Resources;

    internal readonly Dictionary<string, (Baseline baseline, BaselineRef baselineRef)> Baselines;

    public ResourceCache(IList<ResourceRef>? unresolved)
    {
        _TrackedIssues = [];
        _Resources = [];
        Baselines = new Dictionary<string, (Baseline baseline, BaselineRef baselineRef)>(StringComparer.OrdinalIgnoreCase);
        _Unresolved = unresolved ?? [];
    }

    public IEnumerable<ResourceIssue> Issues => _TrackedIssues;

    public IEnumerable<ResourceRef> Unresolved => _Unresolved;

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
            if (!string.IsNullOrEmpty(moduleConfig!.Spec?.Rule?.Baseline))
            {
                var baselineId = ResourceHelper.GetIdString(moduleConfig.Source.Module, moduleConfig.Spec!.Rule.Baseline);
                if (!Baselines.ContainsKey(baselineId))
                    _Unresolved.Add(new BaselineRef(baselineId, ScopeType.Baseline));
            }
            _Resources.Add(moduleConfig);
            return true;
        }
        else if (TrySuppressionGroup(resource, out var suppressionGroup))
        {
            _Resources.Add(suppressionGroup!);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Check for and track resource issues.
    /// </summary>
    /// <returns>If the resource should be ignored then return <c>true</c>, otherwise <c>false</c> is returned.</returns>
    private bool TrackIssue(IResource resource)
    {
        if (TrySuppressionGroup(resource, out var suppressionGroup))
        {
            if (suppressionGroup!.Spec.ExpiresOn.HasValue && suppressionGroup.Spec.ExpiresOn.Value <= DateTime.UtcNow)
            {
                _TrackedIssues.Add(new ResourceIssue(resource, ResourceIssueType.SuppressionGroupExpired));
                return true;
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

    private static bool TryModuleConfig(IResource resource, out ModuleConfigV1? moduleConfig)
    {
        moduleConfig = null;
        if (resource.Kind == ResourceKind.ModuleConfig &&
            !string.IsNullOrEmpty(resource.Source.Module) &&
            StringComparer.OrdinalIgnoreCase.Equals(resource.Source.Module, resource.Name) &&
            resource is ModuleConfigV1 result)
        {
            moduleConfig = result;
            return true;
        }
        return false;
    }

    private static bool TrySelector(IResource resource, out SelectorV1? selector)
    {
        selector = null;
        if (resource.Kind == ResourceKind.Selector && resource is SelectorV1 result)
        {
            selector = result;
            return true;
        }
        return false;
    }

    private static bool TrySuppressionGroup(IResource resource, out SuppressionGroupV1? suppressionGroup)
    {
        suppressionGroup = null;
        if (resource.Kind == ResourceKind.SuppressionGroup && resource is SuppressionGroupV1 result)
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
