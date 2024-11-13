// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule;

#nullable enable

public abstract class ContextBaseTests : BaseTests
{
    internal PipelineContext GetPipelineContext(PSRuleOption? option = default, IPipelineWriter? writer = default, ILanguageScopeSet? languageScope = default, OptionContextBuilder? optionBuilder = default, Source[]? sources = default)
    {
        option ??= GetOption();
        return PipelineContext.New(
            option: option,
            hostContext: null,
            reader: null,
            writer: writer ?? GetTestWriter(option),
            languageScope: languageScope ?? GetLanguageScopeSet(option, sources),
            optionBuilder: optionBuilder ?? new OptionContextBuilder(),
            /*resourceCache: GetResourceCache()*/
            unresolved: null
        );
    }

    internal OptionContextBuilder GetOptionBuilder()
    {
        return new OptionContextBuilder(option: GetOption(), bindTargetName: PipelineHookActions.BindTargetName, bindTargetType: PipelineHookActions.BindTargetType, bindField: PipelineHookActions.BindField);
    }

    internal static ResourceCache GetResourceCache()
    {
        return new ResourceCache(null);
    }

    internal static ILanguageScopeSet GetLanguageScopeSet(PSRuleOption? option = default, Source[]? sources = default)
    {
        var builder = new LanguageScopeSetBuilder();
        builder.Init(option, sources);

        return builder.Build();
    }
}

#nullable restore
