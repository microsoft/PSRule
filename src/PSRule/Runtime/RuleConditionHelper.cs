// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

internal static class RuleConditionHelper
{
    private static readonly RuleConditionResult Empty = new(pass: 0, count: 0, hadErrors: false);

    internal static RuleConditionResult Create(IEnumerable<object> value)
    {
        if (value == null)
            return Empty;

        var count = 0;
        var pass = 0;
        var hasErrors = false;
        foreach (var v in value)
        {
            count++;
            if (v == null)
                continue;

            var baseObject = ExpressionHelpers.GetBaseObject(v);
            if (!(TryAssertResult(baseObject, out var result) || TryBoolean(baseObject, out result)))
            {
                LegacyRunspaceContext.CurrentThread.ErrorInvalidRuleResult();
                hasErrors = true;
            }
            else if (result)
            {
                pass++;
            }
        }
        return new RuleConditionResult(pass, count, hasErrors);
    }

    private static bool TryBoolean(object o, out bool result)
    {
        result = false;
        if (o is not bool b)
            return false;

        result = b;
        return true;
    }

    private static bool TryAssertResult(object o, out bool result)
    {
        result = false;
        if (o is not AssertResult assert)
            return false;

        result = assert.Result;

        // Complete results
        if (LegacyRunspaceContext.CurrentThread.IsScope(RunspaceScope.Rule))
            assert.Complete();

        return true;
    }
}
