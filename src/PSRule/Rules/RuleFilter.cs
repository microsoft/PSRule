using System;
using System.Collections.Generic;

namespace PSRule.Rules
{
    public sealed class RuleFilter
    {
        private HashSet<string> _Name;
        private HashSet<string> _Tag;

        public RuleFilter(IEnumerable<string> name, IEnumerable<string> tag)
        {
            _Name = name == null ? null : new HashSet<string>(name, StringComparer.OrdinalIgnoreCase);
            _Tag = tag == null ? null : new HashSet<string>(tag, StringComparer.OrdinalIgnoreCase);
        }

        // Matches if the Name is contained or any tag is matched
        public bool Match(string name, string[] tag)
        {
            if (_Name == null || _Name.Contains(name))
            {
                if (_Tag == null)
                {
                    return true;
                }

                if (tag == null)
                {
                    return false;
                }

                for (var i = 0; i < tag.Length; i++)
                {
                    if (_Tag.Contains(tag[i]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool Match(RuleBlock block)
        {
            return Match(block.Name, null);
        }
    }
}
