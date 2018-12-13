using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace PSRule.Rules
{
    public sealed class RuleConditionResult
    {
        public RuleConditionResult(bool value)
        {
            Count = 1;
            Pass = value ? 1 : 0;
        }

        public RuleConditionResult(object[] value)
        {
            Process(value);
        }

        public RuleConditionResult(Collection<PSObject> value)
        {
            Process(value);
        }

        public int Count { get; private set; }

        public int Pass { get; private set; }

        public bool AllOf => Count > 0 && Pass == Count;

        public bool AnyOf => Pass > 0;

        public static explicit operator RuleConditionResult(bool value) => new RuleConditionResult(value);

        public static explicit operator RuleConditionResult(object[] value)
        {
            if (value == null)
            {
                return null;
            }

            return new RuleConditionResult(value);
        }

        private void Process(IEnumerable<object> value)
        {
            Count = 0;
            Pass = 0;

            if (value == null)
            {
                return;
            }

            foreach (var v in value)
            {
                Count++;

                if (v is bool && (bool)v)
                {
                    Pass++;
                }
                else if (v is PSObject && (bool)((PSObject)v).BaseObject)
                {
                    Pass++;
                }
            }
        }
    }
}
