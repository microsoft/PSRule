// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// A context that supports configuration values.
/// </summary>
public interface IConfigurableContext
{
    /// <summary>
    /// Get the configuration value for a name.
    /// </summary>
    /// <param name="name">The name of the configuration value.</param>
    /// <param name="value">The configuration vale.</param>
    /// <returns>Returns <c>true</c> if a non-null configuration value is defined. Otherwise returns false.</returns>
    bool TryGetConfigurationValue(string name, out object? value);
}
