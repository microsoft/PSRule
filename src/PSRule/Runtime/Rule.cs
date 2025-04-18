// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

#pragma warning disable CA1822 // Mark members as static

/// <summary>
/// A set of rule properties that are exposed at runtime through the $Rule variable.
/// </summary>
public sealed class Rule
{
    /// <summary>
    /// The name of the currently executing rule.
    /// </summary>
    public string RuleName => LegacyRunspaceContext.CurrentThread.RuleRecord.RuleName;

    /// <summary>
    /// A unique identifer of the currently executing rule.
    /// </summary>
    public string RuleId => LegacyRunspaceContext.CurrentThread.RuleRecord.RuleId;
}

#pragma warning restore CA1822 // Mark members as static

