// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule.Definitions.Expressions;

internal interface IExpressionContext : IBindingContext
{
    ResourceKind Kind { get; }

    SourceFile Source { get; }

    string LanguageScope { get; }

    void Reason(IOperand operand, string text, params object[] args);

    void Debug(string message, params object[] args);

    object Current { get; }

    RunspaceContext Context { get; }

    void PushScope(RunspaceScope scope);

    void PopScope(RunspaceScope scope);
}
