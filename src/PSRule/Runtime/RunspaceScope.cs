// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// The available language scope types.
/// </summary>
[Flags]
public enum RunspaceScope
{
    None = 0,

    Source = 1,

    /// <summary>
    /// Executing a rule.
    /// </summary>
    Rule = 2,

    /// <summary>
    /// Executing a rule precondition.
    /// </summary>
    Precondition = 4,

    /// <summary>
    /// Execution is currently parsing YAML objects.
    /// </summary>
    Resource = 8,

    ConventionBegin = 16,
    ConventionProcess = 32,
    ConventionEnd = 64,
    ConventionInitialize = 128,

    Convention = ConventionInitialize | ConventionBegin | ConventionProcess | ConventionEnd,
    Target = Rule | Precondition | ConventionBegin | ConventionProcess,
    Runtime = Rule | Precondition | Convention,
    All = Source | Rule | Precondition | Resource | Convention,
}
