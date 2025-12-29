// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// An interface for a scope configurations.
/// A scope configuration is not a specific resource type, but a contract that can be implemented by
/// module configurations and in the future bundle configurations.
/// </summary>
public interface IScopeConfig
{
    /// <summary>
    /// The unique identifier.
    /// </summary>
    ResourceId Id { get; }

    /// <summary>
    /// Get configuration.
    /// </summary>
    IDictionary<string, object>? Configuration { get; }
}
