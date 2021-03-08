// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime
{
#pragma warning disable CA1822 // Mark members as static

    /// <summary>
    /// A set of rule properties that are exposed at runtime through the $Rule variable.
    /// </summary>
    public sealed class Rule
    {
        public string RuleName => RunspaceContext.CurrentThread.RuleRecord.RuleName;

        public string RuleId => RunspaceContext.CurrentThread.RuleRecord.RuleId;
    }

#pragma warning restore CA1822 // Mark members as static
}
