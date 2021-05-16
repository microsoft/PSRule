// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using System.Collections;
using System.Collections.Generic;

namespace PSRule.Configuration
{
    public class BaselineOption
    {
        internal sealed class BaselineRef : BaselineOption
        {
            public readonly string Name;

            public BaselineRef(string name)
            {
                Name = name;
            }
        }

        internal sealed class BaselineInline : BaselineOption, IBaselineSpec
        {
            public BaselineInline()
            {
                Binding = new BindingOption();
                Configuration = new ConfigurationOption();
                Rule = new RuleOption();
            }

            public BindingOption Binding { get; set; }

            public ConfigurationOption Configuration { get; set; }

            public ConventionOption Convention { get; set; }

            public RuleOption Rule { get; set; }
        }

        public static implicit operator BaselineOption(Hashtable hashtable)
        {
            return FromHashtable(hashtable);
        }

        public static implicit operator BaselineOption(string value)
        {
            return FromString(value);
        }

        public static BaselineOption FromHashtable(Hashtable hashtable)
        {
            var option = new BaselineInline();
            if (hashtable != null)
            {
                // Build index to allow mapping
                var index = PSRuleOption.BuildIndex(hashtable);
                Load(option, index);
            }
            return option;
        }

        public static BaselineOption FromString(string value)
        {
            return new BaselineRef(value);
        }

        internal static void Load(IBaselineSpec option, EnvironmentHelper env)
        {
            // Binding.Field - currently not supported

            if (env.TryBool("PSRULE_BINDING_IGNORECASE", out bool ignoreCase))
                option.Binding.IgnoreCase = ignoreCase;

            if (env.TryString("PSRULE_BINDING_NAMESEPARATOR", out string nameSeparator))
                option.Binding.NameSeparator = nameSeparator;

            if (env.TryBool("PSRULE_BINDING_PREFERTARGETINFO", out bool preferTargetInfo))
                option.Binding.PreferTargetInfo = preferTargetInfo;

            if (env.TryStringArray("PSRULE_BINDING_TARGETNAME", out string[] targetName))
                option.Binding.TargetName = targetName;

            if (env.TryStringArray("PSRULE_BINDING_TARGETTYPE", out string[] targetType))
                option.Binding.TargetType = targetType;

            if (env.TryBool("PSRULE_BINDING_USEQUALIFIEDNAME", out bool useQualifiedName))
                option.Binding.UseQualifiedName = useQualifiedName;

            if (env.TryStringArray("PSRULE_RULE_INCLUDE", out string[] include))
                option.Rule.Include = include;

            if (env.TryStringArray("PSRULE_RULE_EXCLUDE", out string[] exclude))
                option.Rule.Exclude = exclude;

            // Rule.Tag - currently not supported

            // Process configuration values
            option.Configuration.Load(env);
        }

        /// <summary>
        /// Load matching values
        /// </summary>
        /// <param name="option">A baseline options object to load.</param>
        /// <param name="properties">One or more indexed properties.</param>
        internal static void Load(IBaselineSpec option, Dictionary<string, object> properties)
        {
            if (properties.TryPopValue("Binding.Field", out Hashtable map))
                option.Binding.Field = new FieldMap(map);

            if (properties.TryPopBool("Binding.IgnoreCase", out bool ignoreCase))
                option.Binding.IgnoreCase = ignoreCase;

            if (properties.TryPopString("Binding.NameSeparator", out string nameSeparator))
                option.Binding.NameSeparator = nameSeparator;

            if (properties.TryPopBool("Binding.PreferTargetInfo", out bool preferTargetInfo))
                option.Binding.PreferTargetInfo = preferTargetInfo;

            if (properties.TryPopStringArray("Binding.TargetName", out string[] targetName))
                option.Binding.TargetName = targetName;

            if (properties.TryPopStringArray("Binding.TargetType", out string[] targetType))
                option.Binding.TargetType = targetType;

            if (properties.TryPopValue("Binding.UseQualifiedName", out bool useQualifiedName))
                option.Binding.UseQualifiedName = useQualifiedName;

            if (properties.TryPopStringArray("Rule.Include", out string[] include))
                option.Rule.Include = include;
            if (properties.TryPopStringArray("Rule.Exclude", out string[] exclude))
                option.Rule.Exclude = exclude;

            if (properties.TryPopValue("Rule.Tag", out Hashtable tag))
                option.Rule.Tag = tag;

            // Process configuration values
            option.Configuration.Load(properties);
        }
    }
}
