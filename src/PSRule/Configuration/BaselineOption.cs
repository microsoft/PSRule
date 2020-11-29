// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using System;
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
            if (properties.TryPopValue("binding.field", out Hashtable map))
                option.Binding.Field = new FieldMap(map);

            if (properties.TryPopBool("binding.ignorecase", out bool bvalue))
                option.Binding.IgnoreCase = bvalue;

            if (properties.TryPopValue("binding.nameseparator", out object value))
                option.Binding.NameSeparator = value.ToString();

            if (properties.TryPopValue("binding.targetname", out value))
            {
                if (value.GetType().IsArray)
                    option.Binding.TargetName = ((object[])value).OfType<string>().ToArray();
                else
                    option.Binding.TargetName = new string[] { value.ToString() };
            }
            if (properties.TryPopValue("binding.targettype", out value))
            {
                if (value.GetType().IsArray)
                    option.Binding.TargetType = ((object[])value).OfType<string>().ToArray();
                else
                    option.Binding.TargetType = new string[] { value.ToString() };
            }
            if (properties.TryPopValue("binding.usequalifiedname", out bvalue))
                option.Binding.UseQualifiedName = bvalue;

            if (properties.TryPopValue("rule.include", out value))
            {
                if (value.GetType().IsArray)
                    option.Rule.Include = ((object[])value).OfType<string>().ToArray();
                else
                    option.Rule.Include = new string[] { value.ToString() };
            }
            if (properties.TryPopValue("rule.exclude", out value))
            {
                if (value.GetType().IsArray)
                    option.Rule.Exclude = ((object[])value).OfType<string>().ToArray();
                else
                    option.Rule.Exclude = new string[] { value.ToString() };
            }
            if (properties.TryPopValue("rule.tag" , out Hashtable tag))
                option.Rule.Tag = tag;

            // Process configuration values
            option.Configuration.Load(properties);
        }
    }
}
