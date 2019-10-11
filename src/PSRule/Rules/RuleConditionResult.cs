// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using PSRule.Runtime;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSRule.Rules
{
    public sealed class RuleConditionResult
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
            var count = 0;
            var pass = 0;
            var hasError = false;

            if (value == null)
            {
                return new RuleConditionResult(pass: 0, count: 0, hadErrors: false);
            }

            foreach (var v in value)
            {
                count++;

                if (v == null)
                {
                    continue;
                }
                else if (!(TryAssertResult(v, out bool bresult) || TryBoolean(v, out bresult)))
                {
                    PipelineContext.CurrentThread.ErrorInvaildRuleResult();
                    hasError = true;
                    continue;
                }
                else if (bresult)
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
            {
                return false;
            }

            if (o is bool bresult)
            {
                result = bresult;
                return true;
            }

            if (o is PSObject pso && pso.BaseObject is bool psoresult)
            {
                result = psoresult;
                return true;
            }

            return false;
        }

        private static bool TryAssertResult(object o, out bool result)
        {
            result = false;

            if (o == null)
            {
                return false;
            }

            // Complete results
            if (o is AssertResult aresult)
            {
                result = aresult.Complete();
                return true;
            }

            if (o is PSObject pso && pso.BaseObject is AssertResult psoresult)
            {
                result = psoresult.Complete();
                return true;
            }

            return false;
        }
    }
}
