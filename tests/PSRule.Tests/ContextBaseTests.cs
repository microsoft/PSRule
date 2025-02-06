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
        languageScope ??= GetLanguageScopeSet(sources);
        return PipelineContext.New(
            option: option,
            hostContext: null,
            reader: () => new PipelineInputStream(null, null, null, null, null),
            writer: writer,
            languageScope: languageScope,
            optionBuilder: optionBuilder ?? new OptionContextBuilder(),
            resourceCache: resourceCache ?? GetResourceCache(option, languageScope, sources, writer)
        );
    }

    internal OptionContextBuilder GetOptionBuilder(PSRuleOption? option = default)
    {
        return new OptionContextBuilder(option: option ?? GetOption(), bindTargetName: PipelineHookActions.BindTargetName, bindTargetType: PipelineHookActions.BindTargetType, bindField: PipelineHookActions.BindField);
    }

    internal ResourceCache GetResourceCache(PSRuleOption? option = default, ILanguageScopeSet? languageScope = default, Source[]? sources = default, IPipelineWriter? writer = default)
    {
        return new ResourceCacheBuilder
        (
            writer: writer ?? GetTestWriter(option),
            languageScopeSet: languageScope ?? GetLanguageScopeSet(sources)
        ).Import(sources).Build(unresolved: null);
    }

    internal static ILanguageScopeSet GetLanguageScopeSet(Source[]? sources = default, OptionContextBuilder? optionContextBuilder = default)
    {
        var builder = new LanguageScopeSetBuilder();
        builder.Init(sources);
        var languageScopeSet = builder.Build();

        if (optionContextBuilder != null)
        {
            foreach (var scope in languageScopeSet.Get())
            {
                scope.Configure(optionContextBuilder.Build(scope.Name));
            }
        }

        return languageScopeSet;
    }
}

#nullable restore
