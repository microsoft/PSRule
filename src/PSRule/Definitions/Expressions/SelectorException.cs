// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;

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
