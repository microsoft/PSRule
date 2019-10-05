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

        public static readonly BindingOption Default = new BindingOption
        {
            IgnoreCase = DEFAULT_IGNORECASE
        };

        public BindingOption()
        {
            IgnoreCase = null;
            TargetName = null;
            TargetType = null;
        }

        public BindingOption(BindingOption option)
        {
            IgnoreCase = option.IgnoreCase;
            TargetName = option.TargetName;
            TargetType = option.TargetType;
        }

        public override bool Equals(object obj)
        {
            return obj != null &&
                obj is BindingOption &&
                Equals(obj as BindingOption);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                int hash = 17;
                hash = hash * 23 + (IgnoreCase.HasValue ? IgnoreCase.Value.GetHashCode() : 0);
                hash = hash * 23 + (TargetName != null ? TargetName.GetHashCode() : 0);
                hash = hash * 23 + (TargetType != null ? TargetType.GetHashCode() : 0);
                return hash;
            }
        }

        public bool Equals(BindingOption other)
        {
            return other != null &&
                IgnoreCase == other.IgnoreCase &&
                TargetName == other.TargetName &&
                TargetType == other.TargetType;
        }

        /// <summary>
        /// Determines if custom binding uses ignores case when matching properties.
        /// </summary>
        [DefaultValue(null)]
        public bool? IgnoreCase { get; set; }

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
