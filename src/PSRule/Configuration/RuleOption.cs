// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.ComponentModel;

namespace PSRule.Configuration
{
    public sealed class RuleOption : IEquatable<RuleOption>
    {
        private const bool DEFAULT_INCLUDELOCAL = false;

        internal static readonly RuleOption Default = new RuleOption
        {
            IncludeLocal = DEFAULT_INCLUDELOCAL
        };

        public RuleOption()
        {
            Baseline = null;
            Exclude = null;
            IncludeLocal = null;
            Include = null;
            Tag = null;
        }

        public RuleOption(RuleOption option)
        {
            if (option == null)
                return;

            Baseline = option.Baseline;
            Exclude = option.Exclude;
            IncludeLocal = option.IncludeLocal;
            Include = option.Include;
            Tag = option.Tag;
        }

        public override bool Equals(object obj)
        {
            return obj is RuleOption option && Equals(option);
        }

        public bool Equals(RuleOption other)
        {
            return other != null &&
                Baseline == other.Baseline &&
                Exclude == other.Exclude &&
                IncludeLocal == other.IncludeLocal &&
                Include == other.Include &&
                Tag == other.Tag;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                var hash = 17;
                hash = hash * 23 + (Baseline != null ? Baseline.GetHashCode() : 0);
                hash = hash * 23 + (Exclude != null ? Exclude.GetHashCode() : 0);
                hash = hash * 23 + (IncludeLocal.HasValue ? IncludeLocal.Value.GetHashCode() : 0);
                hash = hash * 23 + (Include != null ? Include.GetHashCode() : 0);
                hash = hash * 23 + (Tag != null ? Tag.GetHashCode() : 0);
                return hash;
            }
        }

        internal static RuleOption Combine(RuleOption o1, RuleOption o2)
        {
            var result = new RuleOption(o1)
            {
                Baseline = o1.Baseline ?? o2.Baseline,
                Exclude = o1.Exclude ?? o2.Exclude,
                IncludeLocal = o1.IncludeLocal ?? o2.IncludeLocal,
                Include = o1.Include ?? o2.Include,
                Tag = o1.Tag ?? o2.Tag
            };
            return result;
        }

        /// <summary>
        /// The name of a baseline to use.
        /// </summary>
        [DefaultValue(null)]
        public string Baseline { get; set; }

        /// <summary>
        /// A set of rules to exclude for execution.
        /// </summary>
        [DefaultValue(null)]
        public string[] Exclude { get; set; }

        /// <summary>
        /// Automatically include all local rules in the search path unless they have been explicitly excluded.
        /// </summary>
        [DefaultValue(null)]
        public bool? IncludeLocal { get; set; }

        /// <summary>
        /// A set of rules to include for execution.
        /// </summary>
        [DefaultValue(null)]
        public string[] Include { get; set; }

        /// <summary>
        /// A set of rule tags to include for execution.
        /// </summary>
        [DefaultValue(null)]
        public Hashtable Tag { get; set; }
    }
}
