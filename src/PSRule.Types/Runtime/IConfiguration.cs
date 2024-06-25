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
    /// Get the specified configuration item as a string if it exists.
    /// </summary>
    /// <param name="configurationKey">The name of the configuration item.</param>
    /// <param name="defaultValue">The default value to use if the configuration item does not exist.</param>
    /// <returns>Returns the configuration item or the specified default value.</returns>
    string? GetStringOrDefault(string configurationKey, string? defaultValue = default);

    /// <summary>
    /// Get the specified configuration item as a boolean if it exists.
    /// </summary>
    /// <param name="configurationKey">The name of the configuration item.</param>
    /// <param name="defaultValue">The default value to use if the configuration item does not exist.</param>
    /// <returns>Returns the configuration item or the specified default value.</returns>
    bool? GetBoolOrDefault(string configurationKey, bool? defaultValue = default);

    /// <summary>
    /// Get the specified configuration item as an integer if it exists.
    /// </summary>
    /// <param name="configurationKey">The name of the configuration item.</param>
    /// <param name="defaultValue">The default value to use if the configuration item does not exist.</param>
    /// <returns>Returns the configuration item or the specified default value.</returns>
    int? GetIntegerOrDefault(string configurationKey, int? defaultValue = default);

    /// <summary>
    /// Get the specified configuration item as a string array.
    /// </summary>
    /// <param name="configurationKey">The name of the configuration item.</param>
    /// <returns>
    /// Returns an array of strings.
    /// If the configuration key does not exist and empty array is returned.
    /// If the configuration key is a string, an array with a single element is returned.
    /// </returns>
    string[] GetStringValues(string configurationKey);

    /// <summary>
    /// Check if specified configuration item is enabled.
    /// </summary>
    /// <remarks>
    /// Use this method to check if a feature is enabled.
    /// </remarks>
    /// <param name="configurationKey">The name of the configuration item.</param>
    /// <returns>Returns <c>true</c> when the configuration item exists and it set to <c>true</c>. Otherwise <c>false</c> is returned.</returns>
    bool IsEnabled(string configurationKey);
}
