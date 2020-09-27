// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
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
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var context = RunspaceContext.CurrentThread;
            if (binder == null || string.IsNullOrEmpty(binder.Name))
            {
                result = null;
                return false;
            }

            // Get from baseline configuration
            if (context.Source.Configuration.TryGetValue(binder.Name, out object value))
            {
                result = value;
                return true;
            }

            // Check if value exists in Rule definition defaults
            if (context.RuleBlock == null || context.RuleBlock.Configuration == null || !context.RuleBlock.Configuration.ContainsKey(binder.Name))
            {
                result = null;
                return false;
            }

            // Get from rule default
            result = context.RuleBlock.Configuration[binder.Name];
            return true;
        }

        public string[] GetStringValues(string configurationKey)
        {
            if (!RunspaceContext.CurrentThread.Source.Configuration.TryGetValue(configurationKey, out object value) || value == null)
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
    }
}
