// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PSRule.Definitions.Expressions;

/// <summary>
/// A base class for an expression exception.
/// </summary>
public abstract class ExpressionException : SelectorException
{
    /// <summary>
    /// Create an empty expression exception.
    /// </summary>
    protected ExpressionException() { }

    /// <summary>
    /// Create an expression exception.
    /// </summary>
    protected ExpressionException(string message)
        : base(message) { }

    /// <summary>
    /// Create an expression exception.
    /// </summary>
    protected ExpressionException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Create an expression exception.
    /// </summary>
    protected ExpressionException(string expression, string message)
        : base(message)
    {
        Expression = expression;
    }

    /// <summary>
    /// Create an expression exception.
    /// </summary>
    protected ExpressionException(string expression, string message, Exception innerException)
        : base(message, innerException)
    {
        Expression = expression;
    }

    /// <summary>
    /// Create an expression exception.
    /// </summary>
    protected ExpressionException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }

    /// <summary>
    /// The related expression.
    /// </summary>
    public string Expression { get; }

    /// <inheritdoc/>
    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        base.GetObjectData(info, context);
    }
}
