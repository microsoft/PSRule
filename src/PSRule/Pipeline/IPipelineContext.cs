// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;

namespace PSRule.Pipeline;

internal interface IPipelineContext : IDisposable
{
    PSRuleOption Option { get; }

    IPipelineWriter Writer { get; }

    //ILanguageScopeCollection LanguageScope { get; }
}
