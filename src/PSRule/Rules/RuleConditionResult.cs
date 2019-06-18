using PSRule.Pipeline;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSRule.Rules
{
    public sealed class RuleConditionResult
    {
        public readonly int Pass;
        public readonly int Count;

        private RuleConditionResult(int pass, int count)
        {
            Pass = pass;
            Count = count;
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

            if (value == null)
            {
                return new RuleConditionResult(pass: 0, count: 0);
            }

            foreach (var v in value)
            {
                count++;

                if (!TryBoolean(v, out bool bresult))
                {
                    PipelineContext.CurrentThread.ErrorInvaildRuleResult();

                    continue;
                }

                if (bresult)
                {
                    pass++;
                }
            }

            return new RuleConditionResult(pass: pass, count: count);
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
    }
}
