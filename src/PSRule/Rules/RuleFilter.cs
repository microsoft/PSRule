﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace PSRule.Rules
{
    /// <summary>
    /// A filter to include or exclude rules from being processed by id or tag.
    /// </summary>
    public sealed class RuleFilter
    {
        private HashSet<string> _RequiredRuleName;
        private Hashtable _RequiredTag;

        /// <summary>
        /// Filter rules by id or tag.
        /// </summary>
        /// <param name="ruleName"></param>
        /// <param name="tag"></param>
        public RuleFilter(IEnumerable<string> ruleName, Hashtable tag)
        {
            _RequiredRuleName = ruleName == null ? null : new HashSet<string>(ruleName, StringComparer.OrdinalIgnoreCase);
            _RequiredTag = tag ?? null;
        }

        /// <summary>
        /// Matches if the RuleId is contained or any tag is matched
        /// </summary>
        /// <returns>Return true if rule is matched, otherwise false.</returns>
        public bool Match(string ruleName, TagSet tag)
        {
            if (_RequiredRuleName == null || _RequiredRuleName.Contains(ruleName))
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
    }
}
