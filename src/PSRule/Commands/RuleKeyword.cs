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

        protected bool GetField(object targetObject, string name, bool caseSensitive, out object value)
        {
            value = null;

            if (targetObject == null)
            {
                value = null;
                return false;
            }

            var comparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

            var type = targetObject.GetType();

            // Handle dictionaries and hashtables
            if (type.IsAssignableFrom(typeof(IDictionary)))
            {
                var dictionary = (IDictionary)targetObject;

                foreach (var k in dictionary.Keys)
                {
                    if (comparer.Equals(name, k))
                    {
                        value = dictionary[k];
                        return true;
                    }
                }
            }
            // Handle PSObjects
            else if (type.IsAssignableFrom(typeof(PSObject)))
            {
                var psobject = (PSObject)targetObject;

                foreach (var p in psobject.Properties)
                {
                    if (comparer.Equals(name, p.Name))
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
                    if (comparer.Equals(name, p.Name))
                    {
                        value = p.GetValue(targetObject);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
