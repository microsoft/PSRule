// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;

namespace PSRule.Pipeline;

#nullable enable

/// <summary>
/// A helper to construct a get pipeline.
/// </summary>
internal sealed class GetRulePipelineBuilder : PipelineBuilderBase, IGetPipelineBuilder
{
    private bool _IncludeDependencies;

    internal GetRulePipelineBuilder(Source[] source, IHostContext hostContext)
        : base(source, hostContext) { }

    /// <inheritdoc/>
    public override IPipelineBuilder Configure(PSRuleOption option)
    {
        if (option == null)
            return this;

        Option.Baseline = new Options.BaselineOption(option.Baseline);
        Option.Execution = GetExecutionOption(option.Execution);
        Option.Output.Culture = GetCulture(option.Output.Culture);
        Option.Output.Format = SuppressFormat(option.Output.Format);
        Option.Requires = new RequiresOption(option.Requires);
        Option.Output.JsonIndent = NormalizeJsonIndentRange(option.Output.JsonIndent);
        if (option.Rule != null)
            Option.Rule = new RuleOption(option.Rule);

        return this;
    }

    /// <inheritdoc/>
    public void IncludeDependencies()
    {
        _IncludeDependencies = true;
    }

    /// <inheritdoc/>
    public override IPipeline? Build(IPipelineWriter? writer = null)
    {
        return !RequireModules() || !RequireSources()
            ? null
            : (IPipeline)new GetRulePipeline(
                pipeline: PrepareContext(PipelineHookActions.Empty, writer: writer ?? PrepareWriter()),
                source: Source,
                includeDependencies: _IncludeDependencies
            );
    }

    private static OutputFormat SuppressFormat(OutputFormat? format)
    {
        return !format.HasValue ||
            !(format == OutputFormat.Wide ||
            format == OutputFormat.Json ||
            format == OutputFormat.Yaml) ? OutputFormat.None : format.Value;
    }
}

#nullable restore
