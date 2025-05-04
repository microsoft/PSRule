// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;
using System.Security.Permissions;
using PSRule.Runtime;

namespace PSRule;

/// <summary>
/// A serialization exception.
/// </summary>
[Serializable]
public sealed class PipelineSerializationException : PipelineException
{
    /// <summary>
    /// Creates a serialization exception.
    /// </summary>
    public PipelineSerializationException()
    {
    }

    /// <summary>
    /// Creates a serialization exception.
    /// </summary>
    internal PipelineSerializationException(EventId eventId, string message, string path, Exception innerException)
        : base(eventId, message, innerException)
    {
        Path = path;
    }

    /// <summary>
    /// Creates a serialization exception.
    /// </summary>
    /// <param name="message">The detail of the exception.</param>
    public PipelineSerializationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a serialization exception.
    /// </summary>
    /// <param name="message">The detail of the exception.</param>
    /// <param name="args">Additional argument to add to the format string.</param>
    internal PipelineSerializationException(string message, params object[] args)
        : base(string.Format(Thread.CurrentThread.CurrentCulture, message, args)) { }

    /// <summary>
    /// Creates a serialization exception.
    /// </summary>
    /// <param name="message">The detail of the exception.</param>
    /// <param name="innerException">A nested exception that caused the issue.</param>
    public PipelineSerializationException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Creates a serialization exception.
    /// </summary>
    private PipelineSerializationException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }

    /// <summary>
    /// The path to the file.
    /// </summary>
    public string? Path { get; }

    /// <inheritdoc/>
    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new ArgumentNullException(nameof(info));

        base.GetObjectData(info, context);
    }
}
