// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Resources;

namespace PSRule.Rules
{
    /// <summary>
    /// A filter to include or exclude rules from being processed by id or tag.
    /// </summary>
    internal sealed class RuleFilter : IResourceFilter
    {
        private readonly string[] _Include;
        private readonly string[] _Excluded;
        private readonly Hashtable _Tag;
        private readonly bool _IncludeLocal;
        private readonly WildcardPattern _WildcardMatch;

        /// <summary>
        /// Filter rules by id or tag.
        /// </summary>
        /// <param name="include">Only accept these rules by name.</param>
        /// <param name="tag">Only accept rules that have these tags.</param>
        /// <param name="exclude">Rule that are always excluded by name.</param>
        public RuleFilter(string[] include, Hashtable tag, string[] exclude, bool? includeLocal)
        {
            _Include = include == null || include.Length == 0 ? null : include;
            _Excluded = exclude == null || exclude.Length == 0 ? null : exclude;
            _Tag = tag ?? null;
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

        internal bool Match(string name, ResourceTags tag)
        {
            return !IsExcluded(name) && IsIncluded(name, tag);
        }

        /// <summary>
        /// Matches if the RuleId is contained or any tag is matched
        /// </summary>
        /// <returns>Return true if rule is matched, otherwise false.</returns>
        public bool Match(IResource resource)
        {
            if (IsExcluded(resource.Name))
                return false;

            if (_IncludeLocal && resource.IsLocalScope())
                return true;

            return IsIncluded(resource.Name, resource.Tags);
        }

        private bool IsExcluded(string name)
        {
            return _Excluded != null && Contains(name, _Excluded);
        }

        private bool IsIncluded(string name, ResourceTags tag)
        {
            if (_Include == null || Contains(name, _Include) || MatchWildcard(name))
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
            return false;
        }

        private bool Contains(string name, string[] set)
        {
            for (var i = 0; set != null && i < set.Length; i++)
                if (ResourceIdEqualityComparer.IdEquals(name, set[i]))
                    return true;

            return false;
        }

        private bool MatchWildcard(string name)
        {
            return _WildcardMatch != null && _WildcardMatch.IsMatch(name);
        }
    }
}
