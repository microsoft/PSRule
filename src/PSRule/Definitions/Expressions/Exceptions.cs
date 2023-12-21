// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;
using System.Security.Permissions;
using PSRule.Pipeline;

namespace PSRule.Definitions.Expressions;

/// <summary>
/// A base class for runtime exceptions.
/// </summary>
public abstract class SelectorException : PipelineException
{
    /// <summary>
    /// Create an empty selector exception.
    /// </summary>
    protected SelectorException()
        : base() { }

    /// <summary>
    /// Create an selector exception.
    /// </summary>
    protected SelectorException(string message)
        : base(message) { }

    /// <summary>
    /// Create an selector exception.
    /// </summary>
    protected SelectorException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Create an selector exception.
    /// </summary>
    protected SelectorException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

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
