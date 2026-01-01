// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Resources;

namespace PSRule.Definitions.Rules;

/// <summary>
/// A filter to include or exclude rules from being processed by id or tag.
/// </summary>
internal sealed class RuleFilter : IResourceFilter
{
    internal readonly ResourceIdReference[]? Include;
    internal readonly ResourceIdReference[]? Excluded;
    internal readonly Hashtable? Tag;
    internal readonly ResourceLabels? Labels;
    internal readonly bool IncludeLocal;
    internal readonly WildcardPattern? WildcardMatch;

    /// <summary>
    /// A scope to limit rule matching.
    /// </summary>
    public string? Scope { get; }

    /// <summary>
    /// Filter rules by id or tag.
    /// </summary>
    /// <param name="include">Only accept these rules by name.</param>
    /// <param name="tag">Only accept rules that have these tags.</param>
    /// <param name="exclude">Rule that are always excluded by name.</param>
    /// <param name="includeLocal">Determine if local rules are automatically included.</param>
    /// <param name="labels">Only accept rules that have these labels.</param>
    /// <param name="scope">Limit to a specific scope.</param>
    public RuleFilter(ResourceIdReference[]? include, Hashtable? tag, ResourceIdReference[]? exclude, bool? includeLocal, ResourceLabels? labels, string? scope)
    {
        Include = include == null || include.Length == 0 ? null : include;
        Excluded = exclude == null || exclude.Length == 0 ? null : exclude;
        Tag = tag ?? null;
        Labels = labels ?? null;
        IncludeLocal = includeLocal ?? RuleOption.Default.IncludeLocal!.Value;
        WildcardMatch = null;
        Scope = scope;

        if (include != null && include.Length > 0 && WildcardPattern.ContainsWildcardCharacters(include[0].Raw))
        {
            if (include.Length > 1)
                throw new NotSupportedException(PSRuleResources.MatchSingleName);

            WildcardMatch = new WildcardPattern(include[0].Raw);
        }
    }

    ResourceKind IResourceFilter.Kind => ResourceKind.Rule;

    /// <summary>
    /// Matches if the RuleId is contained or any tag is matched
    /// </summary>
    /// <returns>Return true if rule is matched, otherwise false.</returns>
    public bool Match(IResource resource)
    {
        var ids = resource.GetIds();
        return !IsExcluded(ids) && (IncludeLocal && resource.IsLocalScope() || IsIncluded(ids, resource.Tags, resource.Labels));
    }

    private bool IsExcluded(IEnumerable<ResourceId> ids)
    {
        if (Excluded == null || Excluded.Length == 0)
            return false;

        foreach (var id in ids)
        {
            if (Contains(id, Excluded))
                return true;
        }
        return false;
    }

    private bool IsIncluded(IEnumerable<ResourceId> ids, IResourceTags tag, IResourceLabels labels)
    {
        foreach (var id in ids)
        {
            if (!string.IsNullOrEmpty(Scope) && !string.Equals(id.Scope, Scope, StringComparison.OrdinalIgnoreCase))
                continue;

            if (Include == null || Contains(id, Include) || MatchWildcard(id.Name))
                return TagEquals(tag) && LabelEquals(labels);
        }
        return false;
    }

    private bool TagEquals(IResourceTags tag)
    {
        if (Tag == null)
            return true;

        if (tag == null || Tag.Count > tag.Count)
            return false;

        foreach (DictionaryEntry entry in Tag)
        {
            if (!tag.Contains(entry.Key, entry.Value))
                return false;
        }
        return true;
    }

    private bool LabelEquals(IResourceLabels labels)
    {
        if (Labels == null)
            return true;

        if (labels == null || Labels.Count > labels.Count)
            return false;

        foreach (var taxon in Labels)
        {
            if (!labels.Contains(taxon.Key, taxon.Value))
                return false;
        }
        return true;
    }

    private static bool Contains(ResourceId id, ResourceIdReference[] set)
    {
        for (var i = 0; set != null && i < set.Length; i++)
        {
            if (ResourceIdEqualityComparer.IdEquals(id, set[i]))
                return true;
        }
        return false;
    }

    private bool MatchWildcard(string name)
    {
        return WildcardMatch != null && WildcardMatch.IsMatch(name);
    }
}
