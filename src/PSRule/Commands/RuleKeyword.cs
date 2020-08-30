// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Rules;
using System;
using System.Collections;
using System.Management.Automation;
using System.Threading;

namespace PSRule.Commands
{
    /// <summary>
    /// A base class for Rule keywords.
    /// </summary>
    internal abstract class RuleKeyword : PSCmdlet
    {
        protected static RuleRecord GetResult()
        {
            return RunspaceContext.CurrentThread.RuleRecord;
        }

        protected static PSObject GetTargetObject()
        {
            return RunspaceContext.CurrentThread.TargetObject;
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

                foreach (var key in dictionary.Keys)
                {
                    if (comparer.Equals(name, key))
                    {
                        value = dictionary[key];
                        return true;
                    }
                }
            }
            // Handle PSObjects
            else if (targetObject is PSObject pso)
            {
                foreach (var prop in pso.Properties)
                {
                    if (comparer.Equals(name, prop.Name))
                    {
                        value = prop.Value;
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

        protected static object GetBaseObject(object value)
        {
            if (value == null)
                return null;

            if (value is PSObject pso)
            {
                var baseObject = pso.BaseObject;
                if (baseObject != null)
                    return baseObject;
            }
            return value;
        }

        protected static void WriteReason(string text)
        {
            RunspaceContext.CurrentThread.WriteReason(text);
        }

        protected static bool TryReason(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            WriteReason(text);
            return true;
        }

        protected static bool IsRuleScope()
        {
            return PipelineContext.CurrentThread.ExecutionScope == ExecutionScope.Condition || PipelineContext.CurrentThread.ExecutionScope == ExecutionScope.Precondition;
        }

        protected static bool IsConditionScope()
        {
            return PipelineContext.CurrentThread.ExecutionScope == ExecutionScope.Condition;
        }

        protected static RuleRuntimeException RuleScopeException(string keyword)
        {
            return new RuleRuntimeException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.KeywordRuleScope, keyword));
        }

        protected static RuleRuntimeException ConditionScopeException(string keyword)
        {
            return new RuleRuntimeException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.KeywordConditionScope, keyword));
        }
    }
}
