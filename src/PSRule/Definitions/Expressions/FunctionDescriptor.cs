// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Definitions.Expressions;

/// <summary>
/// A structure describing a specific function.
/// </summary>
[DebuggerDisplay("Function: {Name}")]
internal sealed class FunctionDescriptor(string name, ExpressionBuilderFn fn) : IFunctionDescriptor
{
    /// <inheritdoc/>
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

    /// <inheritdoc/>
    public ExpressionBuilderFn Fn { get; } = fn ?? throw new ArgumentNullException(nameof(fn));
}
