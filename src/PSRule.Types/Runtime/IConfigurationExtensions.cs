// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace PSRule.Runtime;

/// <summary>
/// Extensions for <see cref="IConfiguration"/> instances.
/// </summary>
public static class IConfigurationExtensions
{
    /// <summary>
    /// Get the specified configuration item as a string if it exists.
    /// </summary>
    /// <param name="configuration">The <see cref="IConfiguration"/> instance.</param>
    /// <param name="configurationKey">The name of the configuration item.</param>
    /// <param name="defaultValue">The default value to use if the configuration item does not exist.</param>
    /// <returns>Returns the configuration item or the specified default value.</returns>
    public static string? GetStringOrDefault(this IConfiguration configuration, string configurationKey, string? defaultValue = default)
    {
        return configuration.TryConfigurationValue(configurationKey, out var value) &&
            value != null &&
            TryString(value, out var result) &&
            result != null ? result : defaultValue;
    }

    /// <summary>
    /// Get the specified configuration item as a boolean if it exists.
    /// </summary>
    /// <param name="configuration">The <see cref="IConfiguration"/> instance.</param>
    /// <param name="configurationKey">The name of the configuration item.</param>
    /// <param name="defaultValue">The default value to use if the configuration item does not exist.</param>
    /// <returns>Returns the configuration item or the specified default value.</returns>
    public static bool? GetBoolOrDefault(this IConfiguration configuration, string configurationKey, bool? defaultValue = default)
    {
        return configuration.TryConfigurationValue(configurationKey, out var value) &&
            value != null &&
            TryBool(value, out var result) &&
            result != null ? result : defaultValue;
    }

    /// <summary>
    /// Get the specified configuration item as an integer if it exists.
    /// </summary>
    /// <param name="configuration">The <see cref="IConfiguration"/> instance.</param>
    /// <param name="configurationKey">The name of the configuration item.</param>
    /// <param name="defaultValue">The default value to use if the configuration item does not exist.</param>
    /// <returns>Returns the configuration item or the specified default value.</returns>
    public static int? GetIntegerOrDefault(this IConfiguration configuration, string configurationKey, int? defaultValue = default)
    {
        return configuration.TryConfigurationValue(configurationKey, out var value) &&
            value != null &&
            TryInt(value, out var result) &&
            result != null ? result : defaultValue;
    }

    /// <summary>
    /// Get the specified configuration item as a string array.
    /// </summary>
    /// <param name="configuration">The <see cref="IConfiguration"/> instance.</param>
    /// <param name="configurationKey">The name of the configuration item.</param>
    /// <returns>
    /// Returns an array of strings.
    /// If the configuration key does not exist and empty array is returned.
    /// If the configuration key is a string, an array with a single element is returned.
    /// </returns>
    public static string[] GetStringValues(this IConfiguration configuration, string configurationKey)
    {
        if (!configuration.TryConfigurationValue(configurationKey, out var value) || value == null)
            return [];

        if (value is string valueT)
            return [valueT];

        if (value is string[] result)
            return result;

        if (value is IEnumerable c)
        {
            var cList = new List<string>();
            foreach (var v in c)
            {
                cList.Add(v.ToString());
            }

            return [.. cList];
        }
        return [value.ToString()];
    }

    /// <summary>
    /// Check if specified configuration item is enabled.
    /// </summary>
    /// <remarks>
    /// Use this method to check if a feature is enabled.
    /// </remarks>
    /// <param name="configuration">The <see cref="IConfiguration"/> instance.</param>
    /// <param name="configurationKey">The name of the configuration item.</param>
    /// <returns>Returns <c>true</c> when the configuration item exists and it set to <c>true</c>. Otherwise <c>false</c> is returned.</returns>
    public static bool IsEnabled(this IConfiguration configuration, string configurationKey)
    {
        return configuration.GetBoolOrDefault(configurationKey) ?? false;
    }

    private static bool TryBool(object o, out bool? value)
    {
        value = default;
        if (o is bool result || (o is string s && bool.TryParse(s, out result)))
        {
            value = result;
            return true;
        }
        return false;
    }

    private static bool TryInt(object o, out int? value)
    {
        value = default;
        if (o is int result || (o is string s && int.TryParse(s, out result)))
        {
            value = result;
            return true;
        }
        return false;
    }

    private static bool TryString(object o, out string? value)
    {
        value = default;
        if (o is string result)
        {
            value = result;
            return true;
        }
        return false;
    }
}
