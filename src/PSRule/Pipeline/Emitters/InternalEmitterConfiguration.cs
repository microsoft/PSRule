// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Emitters;
using PSRule.Options;
using PSRule.Runtime;

namespace PSRule.Pipeline.Emitters;

#nullable enable

/// <summary>
/// An emitter configuration.
/// </summary>
internal sealed class InternalEmitterConfiguration(IConfiguration configuration, IFormatOption formatOption) : IEmitterConfiguration
{
    private readonly IConfiguration _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly IFormatOption _FormatOption = formatOption ?? throw new ArgumentNullException(nameof(formatOption));

    /// <inheritdoc/>
    public string[]? GetFormatTypes(string format, string[]? defaultTypes = null)
    {
        return _FormatOption.TryGetValue(format, out var formatType) &&
            formatType != null &&
            formatType.Type != null ? formatType.Type : defaultTypes;
    }

    /// <inheritdoc/>
    public object? GetValueOrDefault(string configurationKey, object? defaultValue = null)
    {
        return _Configuration.GetValueOrDefault(configurationKey, defaultValue);
    }

    /// <inheritdoc/>
    public bool TryConfigurationValue(string configurationKey, out object? value)
    {
        return _Configuration.TryConfigurationValue(configurationKey, out value);
    }
}

#nullable restore
