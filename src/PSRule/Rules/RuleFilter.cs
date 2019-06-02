using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSRule.Rules
{
    /// <summary>
    /// A filter to include or exclude rules from being processed by id or tag.
    /// </summary>
    public sealed class RuleFilter
    {
        private readonly HashSet<string> _RequiredRuleName;
        private readonly HashSet<string> _ExcludedRuleName;
        private readonly Hashtable _RequiredTag;
        private readonly WildcardPattern _WildcardMatch;

        /// <summary>
        /// Filter rules by id or tag.
        /// </summary>
        /// <param name="ruleName">Only accept these rules by name.</param>
        /// <param name="tag">Only accept rules that have these tags.</param>
        /// <param name="exclude">Rule that are always excluded by name.</param>
        public RuleFilter(string[] ruleName, Hashtable tag, IEnumerable<string> exclude, bool wildcardMatch = false)
        {
            _RequiredRuleName = ruleName == null || ruleName.Length == 0 ? null : new HashSet<string>(ruleName, StringComparer.OrdinalIgnoreCase);
            _ExcludedRuleName = exclude == null ? null : new HashSet<string>(exclude, StringComparer.OrdinalIgnoreCase);
            _RequiredTag = tag ?? null;
            _WildcardMatch = null;

            if (wildcardMatch && ruleName != null && ruleName.Length > 0 && WildcardPattern.ContainsWildcardCharacters(ruleName[0]))
            {
                if (ruleName.Length > 1)
                {
                    throw new NotSupportedException("Wildcard match requires exactly one ruleName");
                }

                _WildcardMatch = new WildcardPattern(ruleName[0]);
            }
        }

        /// <summary>
        /// Matches if the RuleId is contained or any tag is matched
        /// </summary>
        /// <returns>Return true if rule is matched, otherwise false.</returns>
        public bool Match(string ruleName, TagSet tag)
        {
            if (_ExcludedRuleName != null && _ExcludedRuleName.Contains(ruleName))
            {
                return false;
            }

            if (_RequiredRuleName == null || _RequiredRuleName.Contains(ruleName) || MatchWildcard(ruleName: ruleName))
            {
                if (_RequiredTag == null)
                {
                    return true;
                }

                if (tag == null || _RequiredTag.Count > tag.Count)
                {
                    return false;
                }

                foreach (DictionaryEntry entry in _RequiredTag)
                {
                    if (!tag.Contains(entry.Key, entry.Value))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public bool Match(RuleBlock block)
        {
            return Match(block.RuleName, block.Tag);
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
