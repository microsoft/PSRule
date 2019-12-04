// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Rules;
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
            var option = new BaselineInline();

            // Build index to allow mapping
            var index = PSRuleOption.BuildIndex(hashtable);
            Load(option, index);
            return option;
        }

        public static implicit operator BaselineOption(string value)
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
            if (properties.TryPopValue("binding.ignorecase", out object value))
                option.Binding.IgnoreCase = bool.Parse(value.ToString());

            if (properties.TryPopValue("binding.field", out value) && value is Hashtable map)
                option.Binding.Field = new FieldMap(map);

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
            if (properties.TryPopValue("rule.tag" , out value) && value is Hashtable tag)
                option.Rule.Tag = tag;

            // Process configuration values
            if (properties.Count > 0)
            {
                var keys = properties.Keys.ToArray();
                for (var i = 0; i < keys.Length; i++)
                {
                    if (keys[i].Length > 14 && keys[i].StartsWith("Configuration.", StringComparison.OrdinalIgnoreCase))
                        option.Configuration[keys[i].Substring(14)] = properties[keys[i]];
                }
            }
        }
    }
}
