// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Management.Automation;
using PSRule.Definitions;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Rules;
using PSRule.Runtime;

namespace PSRule.Commands;

/// <summary>
/// A base class for Rule keywords.
/// </summary>
internal abstract class RuleKeyword : PSCmdlet
{
    protected static RuleRecord GetResult()
    {
        return LegacyRunspaceContext.CurrentThread.RuleRecord;
    }

    protected static PSObject GetTargetObject()
    {
        return LegacyRunspaceContext.CurrentThread.TargetObject.Value;
    }

    protected static bool GetField(object targetObject, string name, bool caseSensitive, out object value)
    {
        value = null;
        if (targetObject == null)
        {
            value = null;
            return false;
        }

        var comparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        var baseObject = ExpressionHelpers.GetBaseObject(targetObject);
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

    protected static void WriteReason(string path, string text, params object[] args)
    {
        LegacyRunspaceContext.CurrentThread.WriteReason(new ResultReason(LegacyRunspaceContext.CurrentThread.TargetObject.Path, Operand.FromPath(path), text, args));
    }

    protected static bool TryReason(string path, string text, object[] args)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        WriteReason(path, text, args);
        return true;
    }

    protected static bool IsRuleScope()
    {
        return LegacyRunspaceContext.CurrentThread.IsScope(RunspaceScope.Rule) ||
            LegacyRunspaceContext.CurrentThread.IsScope(RunspaceScope.Precondition);
    }

    protected static bool IsConditionScope()
    {
        return LegacyRunspaceContext.CurrentThread.IsScope(RunspaceScope.Rule);
    }

    protected static RuleException RuleScopeException(string keyword)
    {
        return new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.KeywordRuleScope, keyword));
    }

    protected static RuleException ConditionScopeException(string keyword)
    {
        return new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.KeywordConditionScope, keyword));
    }
}
