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
        private readonly string[] _Include;
        private readonly string[] _Excluded;
        private readonly Hashtable _Tag;
        private readonly ResourceTaxa _Taxa;
        private readonly bool _IncludeLocal;
        private readonly WildcardPattern _WildcardMatch;

        /// <summary>
        /// Filter rules by id or tag.
        /// </summary>
        /// <param name="include">Only accept these rules by name.</param>
        /// <param name="tag">Only accept rules that have these tags.</param>
        /// <param name="exclude">Rule that are always excluded by name.</param>
        /// <param name="includeLocal">Determine if local rules are automatically included.</param>
        /// <param name="taxa">Only accept rules that have these taxa.</param>
        public RuleFilter(string[] include, Hashtable tag, string[] exclude, bool? includeLocal, ResourceTaxa taxa)
        {
            _Include = include == null || include.Length == 0 ? null : include;
            _Excluded = exclude == null || exclude.Length == 0 ? null : exclude;
            _Tag = tag ?? null;
            _Taxa = taxa ?? null;
            _IncludeLocal = includeLocal ?? RuleOption.Default.IncludeLocal.Value;
            _WildcardMatch = null;

            if (include != null && include.Length > 0 && WildcardPattern.ContainsWildcardCharacters(include[0]))
            {
                if (include.Length > 1)
                    throw new NotSupportedException(PSRuleResources.MatchSingleName);

                _WildcardMatch = new WildcardPattern(include[0]);
            }
        }

        ResourceKind IResourceFilter.Kind => ResourceKind.Rule;

        internal bool Match(string name, ResourceTags tag, ResourceTaxa taxa)
        {
            return !IsExcluded(new ResourceId[] { ResourceId.Parse(name) }) &&
                IsIncluded(new ResourceId[] { ResourceId.Parse(name) }, tag, taxa);
        }

        /// <summary>
        /// Matches if the RuleId is contained or any tag is matched
        /// </summary>
        /// <returns>Return true if rule is matched, otherwise false.</returns>
        public bool Match(IResource resource)
        {
            var ids = resource.GetIds();
            return !IsExcluded(ids) && (_IncludeLocal && resource.IsLocalScope() || IsIncluded(ids, resource.Tags, resource.Taxa));
        }

        private bool IsExcluded(IEnumerable<ResourceId> ids)
        {
            if (_Excluded == null)
                return false;

            foreach (var id in ids)
            {
                if (Contains(id, _Excluded))
                    return true;
            }
            return false;
        }

        private bool IsIncluded(IEnumerable<ResourceId> ids, ResourceTags tag, ResourceTaxa taxa)
        {
            foreach (var id in ids)
            {
                if (_Include == null || Contains(id, _Include) || MatchWildcard(id.Name))
                    return TagEquals(tag) && TaxaEquals(taxa);
            }
            return false;
        }

        private bool TagEquals(ResourceTags tag)
        {
            if (_Tag == null)
                return true;

            if (tag == null || _Tag.Count > tag.Count)
                return false;

            foreach (DictionaryEntry entry in _Tag)
            {
                if (!tag.Contains(entry.Key, entry.Value))
                    return false;
            }
            return true;
        }

        private bool TaxaEquals(ResourceTaxa taxa)
        {
            if (_Taxa == null)
                return true;

            if (taxa == null || _Taxa.Count > taxa.Count)
                return false;

            foreach (var taxon in _Taxa)
            {
                if (!taxa.Contains(taxon.Key, taxon.Value))
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
            return _WildcardMatch != null && _WildcardMatch.IsMatch(name);
        }
    }
}
