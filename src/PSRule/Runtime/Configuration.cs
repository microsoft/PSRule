// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Dynamic;

namespace PSRule.Runtime;

#nullable enable

/// <summary>
/// A set of rule configuration values that are exposed at runtime and automatically fallback to defaults when not set in configuration.
/// </summary>
public sealed class Configuration : DynamicObject, IScriptRuntimeConfiguration
{
    private readonly LegacyRunspaceContext _Context;

    internal Configuration(LegacyRunspaceContext context)
    {
        _Context = context;
    }

    /// <inheritdoc/>
    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        result = null;
        if (binder == null || string.IsNullOrEmpty(binder.Name))
            return false;

        // Get from configuration
        return TryConfigurationValue(binder.Name, out result);
    }

    /// <inheritdoc/>
    public object? GetValueOrDefault(string configurationKey, object? defaultValue = default)
    {
        return TryConfigurationValue(configurationKey, out var value) && value != null ? value : defaultValue;
    }

    /// <inheritdoc/>
    public string? GetStringOrDefault(string configurationKey, string? defaultValue = default)
    {
        return TryConfigurationValue(configurationKey, out var value) &&
            value != null &&
            TryString(value, out var result) &&
            result != null ? result : defaultValue;
    }

    /// <inheritdoc/>
    public bool? GetBoolOrDefault(string configurationKey, bool? defaultValue = default)
    {
        return TryConfigurationValue(configurationKey, out var value) &&
            value != null &&
            TryBool(value, out var result) &&
            result != null ? result : defaultValue;
    }

    /// <inheritdoc/>
    public int? GetIntegerOrDefault(string configurationKey, int? defaultValue = default)
    {
        return TryConfigurationValue(configurationKey, out var value) &&
            value != null &&
            TryInt(value, out var result) &&
            result != null ? result : defaultValue;
    }

    /// <inheritdoc/>
    public string[] GetStringValues(string configurationKey)
    {
        if (!TryConfigurationValue(configurationKey, out var value) || value == null)
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

    /// <inheritdoc/>
    public bool IsEnabled(string configurationKey)
    {
        return TryConfigurationValue(configurationKey, out var value) &&
            value != null &&
            TryBool(value, out var result) &&
            result == true;
    }

    /// <inheritdoc/>
    public bool TryConfigurationValue(string name, out object? value)
    {
        value = default;
        return _Context != null && _Context.TryGetConfigurationValue(name, out value);
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

#nullable restore
