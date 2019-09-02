using System.Collections;

namespace PSRule.Configuration
{
    public sealed class RuleOption
    {
        public RuleOption()
        {
            Include = null;
            Exclude = null;
            Tag = null;
        }

        public RuleOption(RuleOption option)
        {
            Include = option.Include;
            Exclude = option.Exclude;
            Tag = option.Tag;
        }

        public string[] Include { get; set; }

        public string[] Exclude { get; set; }

        public Hashtable Tag { get; set; }
    }
}
