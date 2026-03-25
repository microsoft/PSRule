// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PSRule.Definitions.Expressions;

/// <summary>
/// An expression argument exception.
/// </summary>
[Serializable]
public sealed class ExpressionArgumentException : ExpressionException
{
    /// <summary>
    /// Create an empty expression argument exception.
    /// </summary>
    public ExpressionArgumentException() { }

    /// <summary>
    /// Create an expression argument exception.
    /// </summary>
    public ExpressionArgumentException(string message)
        : base(message) { }

    /// <summary>
    /// Create an expression argument exception.
    /// </summary>
    public ExpressionArgumentException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Create an expression argument exception.
    /// </summary>
    internal ExpressionArgumentException(string expression, string message)
        : base(expression, message) { }

    private ExpressionArgumentException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }

    /// <inheritdoc/>
    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        base.GetObjectData(info, context);
    }
}
