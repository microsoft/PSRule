// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;

namespace PSRule.Configuration
{
    /// <summary>
    /// Options tht affect property binding of TargetName and TargetType.
    /// </summary>
    public sealed class BindingOption : IEquatable<BindingOption>
    {
        private const bool DEFAULT_IGNORECASE = true;

        internal static readonly BindingOption Default = new BindingOption
        {
            IgnoreCase = DEFAULT_IGNORECASE
        };

        public BindingOption()
        {
            IgnoreCase = null;
            Field = null;
            TargetName = null;
            TargetType = null;
        }

        public BindingOption(BindingOption option)
        {
            IgnoreCase = option.IgnoreCase;
            Field = option.Field;
            TargetName = option.TargetName;
            TargetType = option.TargetType;
        }

        public override bool Equals(object obj)
        {
            return obj is BindingOption option && Equals(option);
        }

        public bool Equals(BindingOption other)
        {
            return other != null &&
                IgnoreCase == other.IgnoreCase &&
                Field == other.Field &&
                TargetName == other.TargetName &&
                TargetType == other.TargetType;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                int hash = 17;
                hash = hash * 23 + (IgnoreCase.HasValue ? IgnoreCase.Value.GetHashCode() : 0);
                hash = hash * 23 + (Field != null ? Field.GetHashCode() : 0);
                hash = hash * 23 + (TargetName != null ? TargetName.GetHashCode() : 0);
                hash = hash * 23 + (TargetType != null ? TargetType.GetHashCode() : 0);
                return hash;
            }
        }

        /// <summary>
        /// Determines if custom binding uses ignores case when matching properties.
        /// </summary>
        [DefaultValue(null)]
        public bool? IgnoreCase { get; set; }

        /// <summary>
        /// One or more custom fields to bind.
        /// </summary>
        [DefaultValue(null)]
        public FieldMap Field { get; set; }

        /// <summary>
        /// One or more property names to use to bind TargetName.
        /// </summary>
        [DefaultValue(null)]
        public string[] TargetName { get; set; }

        /// <summary>
        /// One or more property names to use to bind TargetType.
        /// </summary>
        [DefaultValue(null)]
        public string[] TargetType { get; set; }
    }
}
