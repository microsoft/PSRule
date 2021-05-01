// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PSRule.Configuration
{
    /// <summary>
    /// Options that configure conventions.
    /// </summary>
    public sealed class ConventionOption : IEquatable<ConventionOption>
    {
        internal static readonly ConventionOption Default = new ConventionOption
        {

        };

        public ConventionOption()
        {
            Include = null;
        }

        public ConventionOption(ConventionOption option)
        {
            if (option == null)
                return;

            Include = option.Include;
        }

        public override bool Equals(object obj)
        {
            return obj is ConventionOption option && Equals(option);
        }

        public bool Equals(ConventionOption other)
        {
            return other != null &&
                Include == other.Include;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                int hash = 17;
                hash = hash * 23 + (Include != null ? Include.GetHashCode() : 0);
                return hash;
            }
        }

        internal static ConventionOption Combine(ConventionOption o1, ConventionOption o2)
        {
            var result = new ConventionOption(o1)
            {
                Include = o1.Include ?? o2.Include
            };
            return result;
        }

        [DefaultValue(null)]
        public string[] Include { get; set; }

        internal void Load(EnvironmentHelper env)
        {
            if (env.TryStringArray("PSRULE_CONVENTION_INCLUDE", out string[] include))
                Include = include;
        }

        internal void Load(Dictionary<string, object> index)
        {
            if (index.TryPopStringArray("Convention.Include", out string[] include))
                Include = include;
        }
    }
}
