// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime.ObjectPath;

/// <summary>
/// The default context object used using evaluating a path expression.
/// </summary>
internal sealed class PathExpressionContext : IPathExpressionContext
{
    public PathExpressionContext(object input, bool caseSensitive)
    {
        Input = input;
        CaseSensitive = caseSensitive;
    }

    /// <summary>
    /// The original root object passed into the expression.
    /// </summary>
    public object Input { get; }

    /// <summary>
    /// Determines if member name matching is case-sensitive.
    /// </summary>
    public bool CaseSensitive { get; }
}
