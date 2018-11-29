using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PSRule.Rules
{
    public sealed class RuleConditionResult
    {
        private readonly bool _value;

        public RuleConditionResult(bool value)
        {
            _value = value;
        }

        public RuleConditionResult(object[] value)
        {
            var totalCount = 0;
            var successCount = 0;

            foreach (var v in value)
            {
                totalCount++;

                if (v is bool && (bool)v)
                {
                    successCount++;
                }
            }

            _value = successCount == totalCount;
        }

        public bool Success => _value;

        public static explicit operator RuleConditionResult(bool value) => new RuleConditionResult(value);

        public static explicit operator RuleConditionResult(object[] value)
        {
            if (value == null)
            {
                return null;
            }

            return new RuleConditionResult(value);
        }
    }
}
