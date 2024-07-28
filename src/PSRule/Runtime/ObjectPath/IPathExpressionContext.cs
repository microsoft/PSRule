// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime.ObjectPath;

/// <summary>
/// A context ojbect used using evaluating a path expression.
/// </summary>
internal interface IPathExpressionContext
{
    object Input { get; }

    bool CaseSensitive { get; }
}
