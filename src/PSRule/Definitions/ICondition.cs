// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;

namespace PSRule.Definitions;

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
