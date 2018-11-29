using System;
using System.Collections;
using System.Collections.Generic;

namespace PSRule.Rules
{
    /// <summary>
    /// A filter to include or exclude rules from being processed by name or tag.
    /// </summary>
    public sealed class RuleFilter
    {
        private HashSet<string> _RequiredName;
        private Hashtable _RequiredTag;

        public RuleFilter(IEnumerable<string> name, Hashtable tag)
        {
            _RequiredName = name == null ? null : new HashSet<string>(name, StringComparer.OrdinalIgnoreCase);
            _RequiredTag = tag ?? null;
        }

        // Matches if the Name is contained or any tag is matched
        public bool Match(string name, TagSet tag)
        {
            if (_RequiredName == null || _RequiredName.Contains(name))
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
            return Match(block.Name, block.Tag);
        }
    }
}
