// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// An operand that is compared with PSRule expressions.
/// </summary>
public interface IOperand
{
    /// <summary>
    /// The value of the operand.
    /// </summary>
    object? Value { get; }

    /// <summary>
    /// The type of operand.
    /// </summary>
    OperandKind Kind { get; }

    /// <summary>
    /// The object path to the operand.
    /// </summary>
    string? Path { get; }

    /// <summary>
    /// A logical prefix to add to the object path.
    /// </summary>
    string? Prefix { get; set; }
}
