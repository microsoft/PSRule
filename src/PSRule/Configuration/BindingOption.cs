﻿using System.ComponentModel;

namespace PSRule.Configuration
{
    /// <summary>
    /// Options tht affect property binding of TargetName.
    /// </summary>
    public sealed class BindingOption
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
