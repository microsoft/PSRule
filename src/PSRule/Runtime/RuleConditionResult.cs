// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Management.Automation;

namespace PSRule.Runtime
{
    internal static class RuleConditionHelper
    {
        private readonly static RuleConditionResult Empty = new RuleConditionResult(pass: 0, count: 0, hadErrors: false);

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

                var baseObject = GetBaseObject(v);
                if (!(TryAssertResult(baseObject, out bool result) || TryBoolean(baseObject, out result)))
                {
                    RunspaceContext.CurrentThread.ErrorInvaildRuleResult();
                    hasErrors = true;
                }
                else if (result)
                {
                    pass++;
                }
            }
            return new RuleConditionResult(pass, count, hasErrors);
        }

        private static object GetBaseObject(object o)
        {
            return o is PSObject pso ? pso.BaseObject : o;
        }

        private static bool TryBoolean(object o, out bool result)
        {
            result = false;
            if (!(o is bool bresult))
                return false;

            result = bresult;
            return true;
        }

        private static bool TryAssertResult(object o, out bool result)
        {
            result = false;
            if (!(o is AssertResult assert))
                return false;

            result = assert.Result;

            // Complete results
            if (RunspaceContext.CurrentThread.IsScope(RunspaceScope.Rule))
                assert.Complete();

            return true;
        }
    }

    internal sealed class RuleConditionResult
    {
        public readonly int Pass;
        public readonly int Count;
        public readonly bool HadErrors;

        internal RuleConditionResult(int pass, int count, bool hadErrors)
        {
            Pass = pass;
            Count = count;
            HadErrors = hadErrors;
        }

        public bool AllOf()
        {
            return Count > 0 && Pass == Count;
        }

        public bool AnyOf()
        {
            return Pass > 0;
        }
    }
}
