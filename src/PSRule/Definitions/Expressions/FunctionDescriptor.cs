// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Definitions.Expressions;

/// <summary>
/// A structure describing a specific function.
/// </summary>
[DebuggerDisplay("Function: {Name}")]
internal sealed class FunctionDescriptor : IFunctionDescriptor
{
    public FunctionDescriptor(string name, ExpressionBuilderFn fn)
    {
        Name = name;
        Fn = fn;
    }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public ExpressionBuilderFn Fn { get; }
}
