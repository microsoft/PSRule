using PSRule.Pipeline;
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
            return PipelineContext.CurrentThread.RuleRecord;
        }

        protected PSObject GetTargetObject()
        {
            return PipelineContext.CurrentThread.TargetObject;
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
            var baseObject = GetBaseObject(targetObject);
            var baseType = baseObject.GetType();

            // Handle dictionaries and hashtables
            if (typeof(IDictionary).IsAssignableFrom(baseType))
            {
                var dictionary = (IDictionary)baseObject;

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
            else if (targetObject is PSObject)
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
                foreach (var p in baseType.GetProperties())
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

        protected object GetBaseObject(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is PSObject)
            {
                var baseObject = ((PSObject)value).BaseObject;

                if (baseObject != null)
                {
                    return baseObject;
                }
            }

            return value;
        }
    }
}
