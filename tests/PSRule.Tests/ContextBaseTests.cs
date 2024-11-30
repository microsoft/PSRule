// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule;

#nullable enable

public abstract class ContextBaseTests : BaseTests
{
    internal PipelineContext GetPipelineContext(PSRuleOption? option = default, IPipelineWriter? writer = default, ILanguageScopeSet? languageScope = default, OptionContextBuilder? optionBuilder = default, Source[]? sources = default, ResourceCache? resourceCache = default)
    {
        option ??= GetOption();
        writer ??= GetTestWriter(option);
        languageScope ??= GetLanguageScopeSet(option, sources);
        return PipelineContext.New(
            option: option,
            hostContext: null,
            reader: null,
            writer: writer,
            languageScope: languageScope,
            optionBuilder: optionBuilder ?? new OptionContextBuilder(),
            resourceCache: resourceCache ?? GetResourceCache(option, languageScope, sources, writer)
        );
    }

    internal OptionContextBuilder GetOptionBuilder()
    {
        return new OptionContextBuilder(option: GetOption(), bindTargetName: PipelineHookActions.BindTargetName, bindTargetType: PipelineHookActions.BindTargetType, bindField: PipelineHookActions.BindField);
    }

    internal ResourceCache GetResourceCache(PSRuleOption? option = default, ILanguageScopeSet? languageScope = default, Source[]? sources = default, IPipelineWriter? writer = default)
    {
        return new ResourceCacheBuilder
        (
            writer: writer ?? GetTestWriter(option),
            languageScopeSet: languageScope ?? GetLanguageScopeSet(option, sources)
        ).Import(sources).Build(unresolved: null);
    }

    internal static ILanguageScopeSet GetLanguageScopeSet(PSRuleOption? option = default, Source[]? sources = default)
    {
        var builder = new LanguageScopeSetBuilder();
        builder.Init(option, sources);

        return builder.Build();
    }
}

#nullable restore
