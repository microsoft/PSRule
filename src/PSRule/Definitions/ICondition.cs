// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Data;
using PSRule.Definitions.Expressions;

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
    IConditionResult? If(IExpressionContext expressionContext, ITargetObject o);

    /// <summary>
    /// The action of error to take when execution the condition.
    /// </summary>
    ActionPreference ErrorAction { get; }
}
