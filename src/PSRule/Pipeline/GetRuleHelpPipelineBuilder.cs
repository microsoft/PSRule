// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Options;

namespace PSRule.Pipeline;

#nullable enable

internal sealed class GetRuleHelpPipelineBuilder : PipelineBuilderBase, IHelpPipelineBuilder
{
    private bool _Full;
    private bool _Online;

    internal GetRuleHelpPipelineBuilder(Source[] source, IHostContext hostContext)
        : base(source, hostContext) { }

    /// <inheritdoc/>
    public override IPipelineBuilder Configure(PSRuleOption option)
    {
        if (option == null)
            return this;

        Option.Execution = GetExecutionOption(option.Execution);
        Option.Output.Culture = GetCulture(option.Output.Culture);

        if (option.Rule != null)
            Option.Rule = new RuleOption(option.Rule);

        return this;
    }

    /// <inheritdoc/>
    public void Full()
    {
        _Full = true;
    }

    /// <inheritdoc/>
    public void Online()
    {
        _Online = true;
    }

    /// <inheritdoc/>
    public override IPipeline? Build(IPipelineWriter? writer = null)
    {
        var context = PrepareContext(PipelineHookActions.Empty, writer: writer ?? PrepareWriter());
        if (context == null)
            return null;

        return new GetRuleHelpPipeline(
            context: context,
            source: Source
        );
    }

    protected override PipelineWriter PrepareWriter()
    {
        return new HelpWriter(
            inner: GetOutput(),
            option: Option,
            shouldProcess: ShouldProcess,
            languageMode: Option.Execution.LanguageMode.GetValueOrDefault(ExecutionOption.Default.LanguageMode!.Value),
            inSession: InSession,
            online: _Online,
            full: _Full
        );
    }
}

#nullable restore
