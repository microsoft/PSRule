// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;

namespace PSRule.Definitions;

/// <summary>
/// A result from an language condition.
/// </summary>
public interface IConditionResult
{
    /// <summary>
    /// Determine if the condition had errors.
    /// </summary>
    bool HadErrors { get; }

    /// <summary>
    /// The number of sub-conditions that were evaluated.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// The number of sub-conditions that passed.
    /// </summary>
    int Pass { get; }
}

/// <summary>
/// A language condition.
/// </summary>
public interface ICondition : ILanguageBlock, IDisposable
{
    /// <summary>
    /// Invoke the condition to get a result.
    /// </summary>
    /// <returns>Returns the result of the condition.</returns>
    IConditionResult If();

    /// <summary>
    /// The action of error to take when execution the condition.
    /// </summary>
    ActionPreference ErrorAction { get; }
}
