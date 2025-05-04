// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

#nullable enable

/// <summary>
/// A collections of <see cref="ILanguageScope"/>.
/// </summary>
internal interface ILanguageScopeSet : IDisposable
{
    /// <summary>
    /// Try to get a specific scope by name.
    /// </summary>
    /// <param name="name">The name of the scope.</param>
    /// <param name="scope">The resulting scope instance.</param>
    /// <returns>Returns <c>true</c> when the scope exists. Otherwise returns <c>false</c>.</returns>
    bool TryScope(string? name, out ILanguageScope? scope);

    /// <summary>
    /// Get all the language scopes configured in the collection.
    /// </summary>
    IEnumerable<ILanguageScope> Get();
}

#nullable restore
