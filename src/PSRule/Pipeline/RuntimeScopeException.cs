// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;
using System.Security.Permissions;
using PSRule.Runtime;

namespace PSRule.Pipeline;

/// <summary>
/// An exception thrown by PSRule when a runtime property or method is used outside of the intended scope.
/// Avoid using runtime variables outside of a PSRule pipeline.
/// </summary>
[Serializable]
public sealed class RuntimeScopeException : PipelineException
{
    /// <inheritdoc/>
    public RuntimeScopeException()
    {
    }

    /// <inheritdoc/>
    public RuntimeScopeException(string message) : base(message)
    {
    }

    /// <inheritdoc/>
    public RuntimeScopeException(EventId eventId, string message) : base(eventId, message)
    {
    }

    /// <inheritdoc/>
    public RuntimeScopeException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <inheritdoc/>
    public RuntimeScopeException(EventId eventId, string message, Exception innerException) : base(eventId, message, innerException)
    {
    }

    /// <inheritdoc/>
    private RuntimeScopeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    /// <inheritdoc/>
    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new ArgumentNullException(nameof(info));

        base.GetObjectData(info, context);
    }
}
