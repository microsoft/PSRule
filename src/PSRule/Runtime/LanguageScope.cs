// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime
{
    internal interface ILanguageScope
    {
        string Name { get; }
    }

    internal sealed class LanguageScope : ILanguageScope
    {
        public LanguageScope(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
