// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Emitters;

#pragma warning disable CA1822

/// <summary>
/// An empty configuration for an emitter.
/// </summary>
public sealed class EmptyEmitterConfiguration : IEmitterConfiguration
{
    /// <summary>
    /// An default instance of the empty configuration.
    /// </summary>
    public static readonly EmptyEmitterConfiguration Instance = new();

    /// <inheritdoc/>
    public string[] GetFormatTypes(string format, string[]? defaultTypes = null)
    {
        return defaultTypes ?? [];
    }

    /// <inheritdoc/>
    public bool? GetBoolOrDefault(string configurationKey, bool? defaultValue = null)
    {
        return defaultValue;
    }

    /// <inheritdoc/>
    public int? GetIntegerOrDefault(string configurationKey, int? defaultValue = null)
    {
        return defaultValue;
    }

    /// <inheritdoc/>
    public string? GetStringOrDefault(string configurationKey, string? defaultValue = null)
    {
        return defaultValue;
    }

    /// <inheritdoc/>
    public string[] GetStringValues(string configurationKey)
    {
        return [];
    }

    /// <inheritdoc/>
    public object? GetValueOrDefault(string configurationKey, object? defaultValue = null)
    {
        return defaultValue;
    }

    /// <inheritdoc/>
    public bool IsEnabled(string configurationKey)
    {
        return false;
    }

    /// <inheritdoc/>
    public bool TryConfigurationValue(string configurationKey, out object? value)
    {
        value = null;
        return false;
    }
}

#pragma warning restore CA1822
