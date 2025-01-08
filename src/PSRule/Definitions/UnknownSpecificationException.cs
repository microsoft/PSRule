// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;
using System.Security.Permissions;
using PSRule.Pipeline;

namespace PSRule.Definitions;

/// <summary>
/// An unknown specification exception.
/// </summary>
[Serializable]
public sealed class UnknownSpecificationException : PipelineException
{
    /// <summary>
    /// Create an unknown specification exception.
    /// </summary>
    public UnknownSpecificationException(string message)
        : base(message) { }

    /// <summary>
    /// Create an unknown specification exception.
    /// </summary>
    private UnknownSpecificationException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }

    /// <inheritdoc/>
    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));

        base.GetObjectData(info, context);
    }
}
