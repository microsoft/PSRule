// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Resources;

namespace PSRule.Definitions.Rules
{
    /// <summary>
    /// A filter to include or exclude rules from being processed by id or tag.
    /// </summary>
    internal sealed class RuleFilter : IResourceFilter
    {
        internal readonly string[] Include;
        internal readonly string[] Excluded;
        internal readonly Hashtable Tag;
        internal readonly ResourceLabels Labels;
        internal readonly bool IncludeLocal;
        internal readonly WildcardPattern WildcardMatch;

        /// <summary>
        /// Filter rules by id or tag.
        /// </summary>
        /// <param name="include">Only accept these rules by name.</param>
        /// <param name="tag">Only accept rules that have these tags.</param>
        /// <param name="exclude">Rule that are always excluded by name.</param>
        /// <param name="includeLocal">Determine if local rules are automatically included.</param>
        /// <param name="labels">Only accept rules that have these labels.</param>
        public RuleFilter(string[] include, Hashtable tag, string[] exclude, bool? includeLocal, ResourceLabels labels)
        {
            Include = include == null || include.Length == 0 ? null : include;
            Excluded = exclude == null || exclude.Length == 0 ? null : exclude;
            Tag = tag ?? null;
            Labels = labels ?? null;
            IncludeLocal = includeLocal ?? RuleOption.Default.IncludeLocal.Value;
            WildcardMatch = null;

            if (include != null && include.Length > 0 && WildcardPattern.ContainsWildcardCharacters(include[0]))
            {
                if (include.Length > 1)
                    throw new NotSupportedException(PSRuleResources.MatchSingleName);

                WildcardMatch = new WildcardPattern(include[0]);
            }
        }

        ResourceKind IResourceFilter.Kind => ResourceKind.Rule;

        internal bool Match(string name, ResourceTags tag, ResourceLabels labels)
        {
            return !IsExcluded(new ResourceId[] { ResourceId.Parse(name) }) &&
                IsIncluded(new ResourceId[] { ResourceId.Parse(name) }, tag, labels);
        }

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
            if (Excluded == null)
                return false;

            foreach (var id in ids)
            {
                if (Contains(id, Excluded))
                    return true;
            }
            return false;
        }

        private bool IsIncluded(IEnumerable<ResourceId> ids, ResourceTags tag, ResourceLabels labels)
        {
            foreach (var id in ids)
            {
                if (Include == null || Contains(id, Include) || MatchWildcard(id.Name))
                    return TagEquals(tag) && LabelEquals(labels);
            }
            return false;
        }

        private bool TagEquals(ResourceTags tag)
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

        private bool LabelEquals(ResourceLabels labels)
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

        private static bool Contains(ResourceId id, string[] set)
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
}
