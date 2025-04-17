// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;
using PSRule.Runtime;

namespace PSRule;

/// <summary>
/// A base class for all pipeline exceptions.
/// </summary>
public abstract class PipelineException : Exception
{
    /// <summary>
    /// Initialize a new instance of a PSRule exception.
    /// </summary>
    protected PipelineException()
        : base() { }

    /// <summary>
    /// Initialize a new instance of a PSRule exception.
    /// </summary>
    protected PipelineException(string message)
        : base(message) { }

    /// <summary>
    /// Initialize a new instance of a PSRule exception.
    /// </summary>
    protected PipelineException(EventId eventId, string message)
        : base(message)
    {
        EventId = eventId;
    }

    /// <summary>
    /// Initialize a new instance of a PSRule exception.
    /// </summary>
    protected PipelineException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initialize a new instance of a PSRule exception.
    /// </summary>
    protected PipelineException(EventId eventId, string message, Exception innerException)
        : base(message, innerException)
    {
        EventId = eventId;
    }

    /// <summary>
    /// Initialize a new instance of a PSRule exception.
    /// </summary>
    protected PipelineException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }

    /// <summary>
    /// The event identifier for the exception.
    /// </summary>
    public EventId? EventId { get; }
}
