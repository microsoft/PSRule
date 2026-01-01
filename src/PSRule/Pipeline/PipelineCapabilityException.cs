// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Pipeline;

/// <summary>
/// An exception thrown when a capability is not available.
/// </summary>
[Serializable]
public sealed class PipelineCapabilityException : PipelineException
{
    /// <summary>
    /// Create a new instance of the exception.
    /// </summary>
    public PipelineCapabilityException()
        : base() { }

    /// <summary>
    /// Create a new instance of the exception.
    /// </summary>
    public PipelineCapabilityException(string message)
        : base(message) { }

    /// <summary>
    /// Create a new instance of the exception.
    /// </summary>
    public PipelineCapabilityException(string message, string capability, string? module = null)
        : base(message)
    {
        Capability = capability;
        Module = module;
    }

    /// <summary>
    /// The capability that is not available.
    /// </summary>
    public string? Capability { get; set; }

    /// <summary>
    /// The module that requests the capability.
    /// </summary>
    public string? Module { get; set; }
}
