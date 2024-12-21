// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// The available language scope types.
/// </summary>
[Flags]
public enum RunspaceScope
{
    /// <summary>
    /// Unknown scope.
    /// </summary>
    None = 0,

    /// <summary>
    /// During source discovery.
    /// </summary>
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

    /// <summary>
    /// When a convention is executing the begin block.
    /// </summary>
    ConventionBegin = 16,

    /// <summary>
    /// When a convention is executing the process block.
    /// </summary>
    ConventionProcess = 32,

    /// <summary>
    /// When a convention is executing the end block.
    /// </summary>
    ConventionEnd = 64,

    /// <summary>
    /// When a convention is executing the initialize block.
    /// </summary>
    ConventionInitialize = 128,

    /// <summary>
    /// When any convention block is executing.
    /// </summary>
    Convention = ConventionInitialize | ConventionBegin | ConventionProcess | ConventionEnd,

    /// <summary>
    /// When a runtime block is executing and the target is available.
    /// </summary>
    Target = Rule | Precondition | ConventionBegin | ConventionProcess,

    /// <summary>
    /// When any runtime block is executing within a rule or convention.
    /// </summary>
    Runtime = Rule | Precondition | Convention,

    /// <summary>
    /// All scopes.
    /// </summary>
    All = Source | Rule | Precondition | Resource | Convention,
}
