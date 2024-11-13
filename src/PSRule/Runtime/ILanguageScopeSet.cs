// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

#nullable enable

internal interface ILanguageScopeSet : IDisposable
{
    IEnumerable<ILanguageScope> Get();

    bool TryScope(string? name, out ILanguageScope? scope);

    bool Import(string name);
}

#nullable restore
