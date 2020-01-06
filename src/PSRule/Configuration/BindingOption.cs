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
        private const string DEFAULT_NAMESEPARATOR = "/";
        private const bool DEFAULT_USEQUALIFIEDNAME = false;

        internal static readonly BindingOption Default = new BindingOption
        {
            IgnoreCase = DEFAULT_IGNORECASE,
            NameSeparator = DEFAULT_NAMESEPARATOR,
            UseQualifiedName = DEFAULT_USEQUALIFIEDNAME
        };

        public BindingOption()
        {
            Field = null;
            IgnoreCase = null;
            NameSeparator = null;
            TargetName = null;
            TargetType = null;
            UseQualifiedName = null;
        }

        public BindingOption(BindingOption option)
        {
            Field = option.Field;
            IgnoreCase = option.IgnoreCase;
            NameSeparator = option.NameSeparator;
            TargetName = option.TargetName;
            TargetType = option.TargetType;
            UseQualifiedName = option.UseQualifiedName;
        }

        public override bool Equals(object obj)
        {
            return obj is BindingOption option && Equals(option);
        }

        public bool Equals(BindingOption other)
        {
            return other != null &&
                Field == other.Field &&
                IgnoreCase == other.IgnoreCase &&
                NameSeparator == other.NameSeparator &&
                TargetName == other.TargetName &&
                TargetType == other.TargetType &&
                UseQualifiedName == other.UseQualifiedName;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                int hash = 17;
                hash = hash * 23 + (Field != null ? Field.GetHashCode() : 0);
                hash = hash * 23 + (IgnoreCase.HasValue ? IgnoreCase.Value.GetHashCode() : 0);
                hash = hash * 23 + (NameSeparator != null ? NameSeparator.GetHashCode() : 0);
                hash = hash * 23 + (TargetName != null ? TargetName.GetHashCode() : 0);
                hash = hash * 23 + (TargetType != null ? TargetType.GetHashCode() : 0);
                hash = hash * 23 + (UseQualifiedName.HasValue ? UseQualifiedName.Value.GetHashCode() : 0);
                return hash;
            }
        }

        /// <summary>
        /// One or more custom fields to bind.
        /// </summary>
        [DefaultValue(null)]
        public FieldMap Field { get; set; }

        /// <summary>
        /// Determines if custom binding uses ignores case when matching properties.
        /// </summary>
        [DefaultValue(null)]
        public bool? IgnoreCase { get; set; }

        /// <summary>
        /// Configures the separator to use for building a qualified name.
        /// </summary>
        [DefaultValue(null)]
        public string NameSeparator { get; set; }

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

        /// <summary>
        /// Determines if a qualified TargetName is used.
        /// </summary>
        [DefaultValue(null)]
        public bool? UseQualifiedName { get; set; }
    }
}
