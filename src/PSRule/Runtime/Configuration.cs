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
            // Get from baseline configuration
            if (PipelineContext.CurrentThread.Source.Configuration.TryGetValue(binder.Name, out object value))
            {
                result = value;
                return true;
            }

            // Check if value exists in Rule definition defaults
            if (PipelineContext.CurrentThread.RuleBlock == null || PipelineContext.CurrentThread.RuleBlock.Configuration == null || !PipelineContext.CurrentThread.RuleBlock.Configuration.ContainsKey(binder.Name))
            {
                result = null;
                return false;
            }

            // Get from rule default
            result = PipelineContext.CurrentThread.RuleBlock.Configuration[binder.Name];
            return true;
        }

        public string[] GetStringValues(string configurationKey)
        {
            if (!PipelineContext.CurrentThread.Source.Configuration.TryGetValue(configurationKey, out object value) || value == null)
                return new string[] { };

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
