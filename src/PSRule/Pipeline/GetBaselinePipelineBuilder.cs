// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Definitions.Baselines;

namespace PSRule.Pipeline;

#nullable enable

internal sealed class GetBaselinePipelineBuilder : PipelineBuilderBase
{
    private string[]? _Name;

    internal GetBaselinePipelineBuilder(Source[] source, IHostContext hostContext)
        : base(source, hostContext) { }

    /// <summary>
    /// Filter returned baselines by name.
    /// </summary>
    public new void Name(string[]? name)
    {
        if (name == null || name.Length == 0)
            return;

        _Name = name;
    }

    public override IPipelineBuilder Configure(PSRuleOption option)
    {
        if (option == null)
            return this;

        Option.Baseline = new Options.BaselineOption(option.Baseline);
        Option.Output.As = ResultFormat.Detail;
        Option.Output.Culture = GetCulture(option.Output.Culture);
        Option.Output.Format = SuppressFormat(option.Output.Format);
        Option.Output.JsonIndent = NormalizeJsonIndentRange(option.Output.JsonIndent);
        return this;
    }

    public override IPipeline? Build(IPipelineWriter? writer = null)
    {
        var filter = new BaselineFilter(ResolveBaselineGroup(_Name));
        return new GetBaselinePipeline(
            pipeline: PrepareContext(PipelineHookActions.Empty, writer: writer),
            source: Source,
            filter: filter
        );
    }

    private static OutputFormat SuppressFormat(OutputFormat? format)
    {
        return !format.HasValue ||
            !(format == OutputFormat.Yaml ||
            format == OutputFormat.Json) ? OutputFormat.None : format.Value;
    }
}

#nullable restore
