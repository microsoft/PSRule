// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// Access configuration values at runtime.
/// </summary>
public interface IConfiguration
{
    /// <summary>
    /// Try to the configuration item if it exists.
    /// </summary>
    /// <param name="configurationKey">The name of the configuration item.</param>
    /// <param name="defaultValue">The default value to use if the configuration item does not exist.</param>
    /// <returns>Returns the configuration item or the specified default value.</returns>
    object? GetValueOrDefault(string configurationKey, object? defaultValue = default);

    /// <summary>
    /// Try to get the configuration item if it exists.
    /// </summary>
    /// <param name="configurationKey">The name of the configuration item.</param>
    /// <param name="value">The value of the configuration item if it exists, otherwise this is <c>null</c>.</param>
    /// <returns>Returns <c>true</c> if the configuration value exists.</returns>
    bool TryConfigurationValue(string configurationKey, out object? value);
}
