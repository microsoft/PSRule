// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

namespace PSRule.Runtime
{
    /// <summary>
    /// A set of rule configuration values that are exposed at runtime and automatically failback to defaults when not set in configuration.
    /// </summary>
    public sealed class Configuration : DynamicObject
    {
        private readonly RunspaceContext _Context;

        internal Configuration() { }

        internal Configuration(RunspaceContext context)
        {
            _Context = context;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            if (binder == null || string.IsNullOrEmpty(binder.Name))
                return false;

            // Get from configuration
            return TryGetValue(binder.Name, out result);
        }

        public string[] GetStringValues(string configurationKey)
        {
            if (!TryGetValue(configurationKey, out object value) || value == null)
                return System.Array.Empty<string>();

            if (value is string valueT)
                return new string[] { valueT };

            if (value is string[] result)
                return result;

            if (value is IEnumerable c)
            {
                var cList = new List<string>();
                foreach (var v in c)
                    cList.Add(v.ToString());

                return cList.ToArray();
            }
            return new string[] { value.ToString() };
        }

        public object GetValueOrDefault(string configurationKey, object defaultValue)
        {
            if (!TryGetValue(configurationKey, out object value) || value == null)
                return defaultValue;

            return value;
        }

        public bool GetBoolOrDefault(string configurationKey, bool defaultValue)
        {
            if (!TryGetValue(configurationKey, out object value) || !TryBool(value, out bool result))
                return defaultValue;

            return result;
        }

        public int GetIntegerOrDefault(string configurationKey, int defaultValue)
        {
            if (!TryGetValue(configurationKey, out object value) || !TryInt(value, out int result))
                return defaultValue;

            return result;
        }

        private bool TryGetValue(string name, out object value)
        {
            value = null;
            var context = GetContext();
            if (context == null)
                return false;

            // Get from baseline configuration
            if (context.Source.Configuration.TryGetValue(name, out object result))
            {
                value = result;
                return true;
            }

            // Check if value exists in Rule definition defaults
            if (context.RuleBlock == null || context.RuleBlock.Configuration == null || !context.RuleBlock.Configuration.ContainsKey(name))
                return false;

            // Get from rule default
            value = context.RuleBlock.Configuration[name];
            return true;
        }

        private static bool TryBool(object o, out bool value)
        {
            value = default;
            if (o is bool result || (o is string svalue && bool.TryParse(svalue, out result)))
            {
                value = result;
                return true;
            }
            return false;
        }

        private static bool TryInt(object o, out int value)
        {
            value = default;
            if (o is int result || (o is string svalue && int.TryParse(svalue, out result)))
            {
                value = result;
                return true;
            }
            return false;
        }

        private RunspaceContext GetContext()
        {
            return _Context ?? RunspaceContext.CurrentThread;
        }
    }
}
