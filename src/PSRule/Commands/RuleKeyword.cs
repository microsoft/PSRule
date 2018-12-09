using PSRule.Rules;
using System;
using System.Collections;
using System.Management.Automation;

namespace PSRule.Commands
{
    /// <summary>
    /// A base class for Rule keywords.
    /// </summary>
    internal abstract class RuleKeyword : PSCmdlet
    {
        protected RuleRecord GetResult()
        {
            return GetVariableValue("Rule") as RuleRecord;
        }

        protected bool GetField(object obj, string name, out object value)
        {
            value = null;

            if (obj == null)
            {
                value = null;
                return false;
            }

            var type = obj.GetType();

            // Handle dictionaries and hashtables
            if (type.IsAssignableFrom(typeof(IDictionary)))
            {
                var dictionary = (IDictionary)obj;

                foreach (var k in dictionary.Keys)
                {
                    if (StringComparer.OrdinalIgnoreCase.Equals(name, k))
                    {
                        value = dictionary[k];
                        return true;
                    }
                }
            }
            // Handle PSObjects
            else if (type.IsAssignableFrom(typeof(PSObject)))
            {
                var psobject = (PSObject)obj;

                foreach (var p in psobject.Properties)
                {
                    if (StringComparer.OrdinalIgnoreCase.Equals(name, p.Name))
                    {
                        value = p.Value;
                        return true;
                    }
                }
            }
            // Handle all other CLR types
            else
            {
                foreach (var p in type.GetProperties())
                {
                    if (StringComparer.OrdinalIgnoreCase.Equals(name, p.Name))
                    {
                        value = p.GetValue(obj);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
