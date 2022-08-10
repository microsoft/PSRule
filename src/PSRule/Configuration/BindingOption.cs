// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;

namespace PSRule.Configuration
{
    /// <summary>
    /// Options that affect property binding of TargetName and TargetType.
    /// </summary>
    public sealed class BindingOption : IEquatable<BindingOption>
    {
        private const bool DEFAULT_IGNORECASE = true;
        private const bool DEFAULT_PREFERTARGETINFO = false;
        private const string DEFAULT_NAMESEPARATOR = "/";
        private const bool DEFAULT_USEQUALIFIEDNAME = false;

        internal static readonly BindingOption Default = new()
        {
            IgnoreCase = DEFAULT_IGNORECASE,
            NameSeparator = DEFAULT_NAMESEPARATOR,
            PreferTargetInfo = DEFAULT_PREFERTARGETINFO,
            UseQualifiedName = DEFAULT_USEQUALIFIEDNAME
        };

        /// <summary>
        /// Creates an empty binding option.
        /// </summary>
        public BindingOption()
        {
            Field = null;
            IgnoreCase = null;
            NameSeparator = null;
            PreferTargetInfo = null;
            TargetName = null;
            TargetType = null;
            UseQualifiedName = null;
        }

        /// <summary>
        /// Creates a binding option by copying an existing instance.
        /// </summary>
        /// <param name="option">The option instance to copy.</param>
        public BindingOption(BindingOption option)
        {
            if (option == null)
                return;

            Field = option.Field;
            IgnoreCase = option.IgnoreCase;
            NameSeparator = option.NameSeparator;
            PreferTargetInfo = option.PreferTargetInfo;
            TargetName = option.TargetName;
            TargetType = option.TargetType;
            UseQualifiedName = option.UseQualifiedName;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is BindingOption option && Equals(option);
        }

        /// <inheritdoc/>
        public bool Equals(BindingOption other)
        {
            return other != null &&
                Field == other.Field &&
                IgnoreCase == other.IgnoreCase &&
                NameSeparator == other.NameSeparator &&
                PreferTargetInfo == other.PreferTargetInfo &&
                TargetName == other.TargetName &&
                TargetType == other.TargetType &&
                UseQualifiedName == other.UseQualifiedName;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                var hash = 17;
                hash = hash * 23 + (Field != null ? Field.GetHashCode() : 0);
                hash = hash * 23 + (IgnoreCase.HasValue ? IgnoreCase.Value.GetHashCode() : 0);
                hash = hash * 23 + (NameSeparator != null ? NameSeparator.GetHashCode() : 0);
                hash = hash * 23 + (PreferTargetInfo.HasValue ? PreferTargetInfo.Value.GetHashCode() : 0);
                hash = hash * 23 + (TargetName != null ? TargetName.GetHashCode() : 0);
                hash = hash * 23 + (TargetType != null ? TargetType.GetHashCode() : 0);
                hash = hash * 23 + (UseQualifiedName.HasValue ? UseQualifiedName.Value.GetHashCode() : 0);
                return hash;
            }
        }

        /// <summary>
        /// Merge two option instances by repacing any unset properties from <paramref name="o1"/> with <paramref name="o2"/> values.
        /// Values from <paramref name="o1"/> that are set are not overridden.
        /// </summary>
        internal static BindingOption Combine(BindingOption o1, BindingOption o2)
        {
            var result = new BindingOption(o1)
            {
                Field = o1.Field ?? o2.Field,
                IgnoreCase = o1.IgnoreCase ?? o2.IgnoreCase,
                NameSeparator = o1.NameSeparator ?? o2.NameSeparator,
                PreferTargetInfo = o1.PreferTargetInfo ?? o2.PreferTargetInfo,
                TargetName = o1.TargetName ?? o2.TargetName,
                TargetType = o1.TargetType ?? o2.TargetType,
                UseQualifiedName = o1.UseQualifiedName ?? o2.UseQualifiedName
            };
            return result;
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
        /// Determines if binding prefers target info provided by the object over custom configuration.
        /// </summary>
        [DefaultValue(null)]
        public bool? PreferTargetInfo { get; set; }

        /// <summary>
        /// Property names to use to bind TargetName.
        /// </summary>
        [DefaultValue(null)]
        public string[] TargetName { get; set; }

        /// <summary>
        /// Property names to use to bind TargetType.
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
