// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

internal interface ILanguageScopeCollection : IDisposable
{
    IEnumerable<ILanguageScope> Get();

    bool TryScope(string name, out ILanguageScope scope);

    bool Import(string name);
}
