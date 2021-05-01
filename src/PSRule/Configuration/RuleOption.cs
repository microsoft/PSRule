// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.ComponentModel;

namespace PSRule.Configuration
{
    public sealed class RuleOption : IEquatable<RuleOption>
    {
        public RuleOption()
        {
            Include = null;
            Exclude = null;
            Tag = null;
        }

        public RuleOption(RuleOption option)
        {
            if (option == null)
                return;

            Include = option.Include;
            Exclude = option.Exclude;
            Tag = option.Tag;
        }

        public override bool Equals(object obj)
        {
            return obj is RuleOption option && Equals(option);
        }

        public bool Equals(RuleOption other)
        {
            return other != null &&
                Include == other.Include &&
                Exclude == other.Exclude &&
                Tag == other.Tag;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                int hash = 17;
                hash = hash * 23 + (Include != null ? Include.GetHashCode() : 0);
                hash = hash * 23 + (Exclude != null ? Exclude.GetHashCode() : 0);
                hash = hash * 23 + (Tag != null ? Tag.GetHashCode() : 0);
                return hash;
            }
        }

        internal static RuleOption Combine(RuleOption o1, RuleOption o2)
        {
            var result = new RuleOption(o1)
            {
                Include = o1.Include ?? o2.Include,
                Exclude = o1.Exclude ?? o2.Exclude,
                Tag = o1.Tag ?? o2.Tag
            };
            return result;
        }

        /// <summary>
        /// A set of rules to include for execution.
        /// </summary>
        [DefaultValue(null)]
        public string[] Include { get; set; }

        /// <summary>
        /// A set of rules to exclude for execution.
        /// </summary>
        [DefaultValue(null)]
        public string[] Exclude { get; set; }

        /// <summary>
        /// A set of rule tags to include for execution.
        /// </summary>
        [DefaultValue(null)]
        public Hashtable Tag { get; set; }
    }
}
