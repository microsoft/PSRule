// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Load matching values
        /// </summary>
        /// <param name="option">A baseline options object to load.</param>
        /// <param name="properties">One or more indexed properties.</param>
        internal static void Load(IBaselineSpec option, Dictionary<string, object> properties)
        {
            if (properties.TryPopValue("Binding.Field", out Hashtable map))
                option.Binding.Field = new FieldMap(map);

            if (properties.TryPopBool("Binding.IgnoreCase", out bool bvalue))
                option.Binding.IgnoreCase = bvalue;

            if (properties.TryPopBool("Binding.PreferTargetInfo", out bvalue))
                option.Binding.PreferTargetInfo = bvalue;

            if (properties.TryPopValue("Binding.NameSeparator", out object value))
                option.Binding.NameSeparator = value.ToString();

            if (properties.TryPopValue("Binding.TargetName", out value))
            {
                if (value.GetType().IsArray)
                    option.Binding.TargetName = ((object[])value).OfType<string>().ToArray();
                else
                    option.Binding.TargetName = new string[] { value.ToString() };
            }
            if (properties.TryPopValue("Binding.targettype", out value))
            {
                if (value.GetType().IsArray)
                    option.Binding.TargetType = ((object[])value).OfType<string>().ToArray();
                else
                    option.Binding.TargetType = new string[] { value.ToString() };
            }
            if (properties.TryPopValue("Binding.UseQualifiedName", out bvalue))
                option.Binding.UseQualifiedName = bvalue;

            if (properties.TryPopValue("Rule.Include", out value))
            {
                if (value.GetType().IsArray)
                    option.Rule.Include = ((object[])value).OfType<string>().ToArray();
                else
                    option.Rule.Include = new string[] { value.ToString() };
            }
            if (properties.TryPopValue("Rule.Exclude", out value))
            {
                if (value.GetType().IsArray)
                    option.Rule.Exclude = ((object[])value).OfType<string>().ToArray();
                else
                    option.Rule.Exclude = new string[] { value.ToString() };
            }
            if (properties.TryPopValue("Rule.Tag", out Hashtable tag))
                option.Rule.Tag = tag;

            // Process configuration values
            option.Configuration.Load(properties);
        }
    }
}
