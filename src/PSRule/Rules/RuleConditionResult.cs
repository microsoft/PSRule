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

                if (v is bool && (bool)v)
                {
                    pass++;
                }
                else if (v is PSObject && (bool)((PSObject)v).BaseObject)
                {
                    pass++;
                }
            }

            return new RuleConditionResult(pass: pass, count: count);
        }
    }
}
