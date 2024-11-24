// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;

namespace PSRule.Runtime;

#nullable enable

/// <summary>
/// An internal configuration based on a dictionary.
/// </summary>
/// <param name="configuration">A dictionary of key/ value pairs.</param>
internal sealed class InternalConfiguration(IDictionary<string, object> configuration) : IConfiguration
{
    private readonly ReadOnlyDictionary<string, object> _Configuration = new(configuration);

    public object? GetValueOrDefault(string configurationKey, object? defaultValue = null)
    {
        return TryConfigurationValue(configurationKey, out var value) ? value : defaultValue;
    }

    public bool TryConfigurationValue(string configurationKey, out object? value)
    {
        value = null;
        return !string.IsNullOrEmpty(configurationKey) && _Configuration.TryGetValue(configurationKey, out value);
    }
}

#nullable restore
