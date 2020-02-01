// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSRule.Runtime
{
    internal sealed class RuleConditionResult
    {
        public readonly int Pass;
        public readonly int Count;
        public readonly bool HadErrors;

        private RuleConditionResult(int pass, int count, bool hadErrors)
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

        internal static RuleConditionResult Create(IEnumerable<object> value)
        {
            if (value == null)
                return new RuleConditionResult(pass: 0, count: 0, hadErrors: false);

            var count = 0;
            var pass = 0;
            var hasError = false;
            foreach (var v in value)
            {
                count++;
                if (v == null)
                {
                    continue;
                }
                else if (!(TryAssertResult(v, out bool result) || TryBoolean(v, out result)))
                {
                    RunspaceContext.CurrentThread.ErrorInvaildRuleResult();
                    hasError = true;
                    continue;
                }
                else if (result)
                {
                    pass++;
                }
            }
            return new RuleConditionResult(pass: pass, count: count, hadErrors: hasError);
        }

        private static bool TryBoolean(object o, out bool result)
        {
            result = false;
            if (o == null)
                return false;

            var baseObject = o is PSObject pso ? pso.BaseObject : o;
            if (!(baseObject is bool bresult))
                return false;

            result = bresult;
            return true;
        }

        private static bool TryAssertResult(object o, out bool result)
        {
            result = false;
            if (o == null)
                return false;

            var baseObject = o is PSObject pso ? pso.BaseObject : o;
            if (!(baseObject is AssertResult assert))
                return false;

            result = assert.Result;

            // Complete results
            if (PipelineContext.CurrentThread.ExecutionScope == ExecutionScope.Condition)
                assert.Complete();

            return true;
        }
    }
}
