// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Runtime;

namespace PSRule.Emitters;

/// <summary>
/// Scoped configuration for an emitter.
/// </summary>
public interface IEmitterConfiguration : IConfiguration
{
    /// <summary>
    /// Get the types that are configured for the format.
    /// </summary>
    /// <param name="format">The name of the format that will be referenced.</param>
    /// <param name="defaultTypes">Configures the default types that are used if the format is not set.</param>
    string[]? GetFormatTypes(string format, string[]? defaultTypes = null);

    /// <summary>
    /// Get a list of replacement tokens configured for the format.
    /// </summary>
    /// <param name="format">The name of the format that will be referenced.</param>
    /// <returns>Returns a list of key value pairs if any replacement tokens are defined. Otherwise <c>null</c> is returned.</returns>
    KeyValuePair<string, string>[]? GetFormatReplacementTokens(string format);
}
