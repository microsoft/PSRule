// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions.Expressions;

/// <summary>
/// A structure describing a specific function.
/// </summary>
internal interface IFunctionDescriptor
{
    /// <summary>
    /// The name of the function.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The function delegate.
    /// </summary>
    ExpressionBuilderFn Fn { get; }
}
