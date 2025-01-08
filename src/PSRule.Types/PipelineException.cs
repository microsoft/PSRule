// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;

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
    protected PipelineException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initialize a new instance of a PSRule exception.
    /// </summary>
    protected PipelineException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
