// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// The type of operand that is compared with the expression.
/// </summary>
public enum OperandKind
{
    /// <summary>
    /// Unknown.
    /// </summary>
    None = 0,

    /// <summary>
    /// An object path.
    /// </summary>
    Path = 1,

    /// <summary>
    /// The object target type.
    /// </summary>
    Type = 2,

    /// <summary>
    /// The object target name.
    /// </summary>
    Name = 3,

    /// <summary>
    /// The object source information.
    /// </summary>
    Source = 4,

    /// <summary>
    /// The target object itself.
    /// </summary>
    Target = 5,

    /// <summary>
    /// A literal value or function.
    /// </summary>
    Value = 6,

    /// <summary>
    /// The object scope.
    /// </summary>
    Scope = 7
}
