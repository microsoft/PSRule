// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;
using PSRule.Runtime;

namespace PSRule.Definitions.Expressions;

internal interface IExpressionContext : IBindingContext, IConfigurableContext
{
    ResourceKind Kind { get; }

    ISourceFile Source { get; }

    string LanguageScope { get; }

    void Reason(IOperand operand, string text, params object[] args);

    void Debug(string message, params object[] args);

    ITargetObject Current { get; }

    /// <summary>
    /// The current rule identifier.
    /// This is only applicable for suppression groups, otherwise this is null.
    /// </summary>
    ResourceId? RuleId { get; }

    LegacyRunspaceContext Context { get; }

    void PushScope(RunspaceScope scope);

    void PopScope(RunspaceScope scope);

    /// <summary>
    /// Evaluate a target object to determine if it met the conditions of the selector.
    /// </summary>
    /// <param name="id">The resource identifier of the selector to run.</param>
    /// <param name="o">The target object to evaluate with the selector.</param>
    /// <returns>Returns <c>true</c> if the target object met the conditions of the selector.</returns>
    bool TrySelector(ResourceId id, ITargetObject o);
}
