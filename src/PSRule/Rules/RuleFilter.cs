// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSRule.Rules
{
    /// <summary>
    /// A filter to include or exclude rules from being processed by id or tag.
    /// </summary>
    public sealed class RuleFilter : IResourceFilter
    {
        private readonly HashSet<string> _Include;
        private readonly HashSet<string> _Excluded;
        private readonly Hashtable _Tag;
        private readonly WildcardPattern _WildcardMatch;

        /// <summary>
        /// Filter rules by id or tag.
        /// </summary>
        /// <param name="include">Only accept these rules by name.</param>
        /// <param name="tag">Only accept rules that have these tags.</param>
        /// <param name="exclude">Rule that are always excluded by name.</param>
        public RuleFilter(string[] include, Hashtable tag, string[] exclude)
        {
            _Include = include == null || include.Length == 0 ? null : new HashSet<string>(include, StringComparer.OrdinalIgnoreCase);
            _Excluded = exclude == null || exclude.Length == 0 ? null : new HashSet<string>(exclude, StringComparer.OrdinalIgnoreCase);
            _Tag = tag ?? null;
            _WildcardMatch = null;

            if (include != null && include.Length > 0 && WildcardPattern.ContainsWildcardCharacters(include[0]))
            {
                if (include.Length > 1)
                    throw new NotSupportedException(PSRuleResources.MatchSingleName);

                _WildcardMatch = new WildcardPattern(include[0]);
            }
        }

        /// <summary>
        /// Matches if the RuleId is contained or any tag is matched
        /// </summary>
        /// <returns>Return true if rule is matched, otherwise false.</returns>
        public bool Match(string name, TagSet tag)
        {
            if (_Excluded != null && _Excluded.Contains(name))
                return false;

            if (_Include == null || _Include.Contains(name) || MatchWildcard(ruleName: name))
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

        private bool MatchWildcard(string ruleName)
        {
            if (_WildcardMatch == null)
            {
                return false;
            }

            return _WildcardMatch.IsMatch(ruleName);
        }
    }
}
