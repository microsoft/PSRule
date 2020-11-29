// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;

namespace PSRule.Runtime
{
#pragma warning disable CA1822 // Mark members as static

    /// <summary>
    /// A set of rule properties that are exposed at runtime through the $Rule variable.
    /// </summary>
    public sealed class Rule
    {
        public string RuleName
        {
            get
            {
                return RunspaceContext.CurrentThread.RuleRecord.RuleName;
            }
        }

        public string RuleId
        {
            get
            {
                return RunspaceContext.CurrentThread.RuleRecord.RuleId;
            }
        }
    }

#pragma warning restore CA1822 // Mark members as static
}
