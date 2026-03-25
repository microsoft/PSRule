// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PSRule.Definitions.Expressions;

/// <summary>
/// An expression reference exception.
/// </summary>
[Serializable]
public sealed class ExpressionReferenceException : SelectorException
{
    /// <summary>
    /// Create an empty expression reference exception.
    /// </summary>
    public ExpressionReferenceException() { }

    /// <summary>
    /// Create an expression reference exception.
    /// </summary>
    public ExpressionReferenceException(string message)
        : base(message) { }

    /// <summary>
    /// Create an expression reference exception.
    /// </summary>
    public ExpressionReferenceException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Create an expression reference exception.
    /// </summary>
    internal ExpressionReferenceException(string expression, string message)
        : base(message)
    {
        Expression = expression;
    }

    /// <summary>
    /// Create an expression reference exception.
    /// </summary>
    internal ExpressionReferenceException(string expression, string message, Exception innerException)
        : base(message, innerException)
    {
        Expression = expression;
    }

    private ExpressionReferenceException(SerializationInfo info, StreamingContext context)
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
