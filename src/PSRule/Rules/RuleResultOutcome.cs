using System;

namespace PSRule.Rules
{
    [Flags]
    public enum RuleResultOutcome : byte
    {
        None = 0,

        Failed = 1,

        Passed = 2,

        Error = 4,

        Inconclusive = 8,

        InProgress = 16,

        Default = Failed | Passed | Error,

        All = 255
    }
}
