// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PSRule.Definitions.Expressions;

/// <summary>
/// An expression parser exception.
/// </summary>
[Serializable]
public sealed class ExpressionParseException : SelectorException
{
    /// <summary>
    /// Create an empty expression parse exception.
    /// </summary>
    public ExpressionParseException() { }

    /// <summary>
    /// Create an expression parse exception.
    /// </summary>
    public ExpressionParseException(string message)
        : base(message) { }

    /// <summary>
    /// Create an expression parse exception.
    /// </summary>
    public ExpressionParseException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Create an expression parse exception.
    /// </summary>
    internal ExpressionParseException(string expression, string message)
        : base(message)
    {
        Expression = expression;
    }

    /// <summary>
    /// Create an expression parse exception.
    /// </summary>
    internal ExpressionParseException(string expression, string message, Exception innerException)
        : base(message, innerException)
    {
        Expression = expression;
    }

    private ExpressionParseException(SerializationInfo info, StreamingContext context)
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
