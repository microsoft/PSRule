// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Definitions.Baselines;

namespace PSRule.Pipeline;

#nullable enable

internal sealed class ExportBaselinePipelineBuilder : PipelineBuilderBase
{
    private string[] _Name;

    internal ExportBaselinePipelineBuilder(Source[] source, IHostContext hostContext)
        : base(source, hostContext) { }

    /// <summary>
    /// Filter returned baselines by name.
    /// </summary>
    public new void Name(string[] name)
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
        Option.Output.Format = option.Output.Format ?? OutputOption.Default.Format;
        Option.Output.Encoding = option.Output.Encoding ?? OutputOption.Default.Encoding;
        Option.Output.Path = option.Output.Path ?? OutputOption.Default.Path;
        Option.Output.JsonIndent = NormalizeJsonIndentRange(option.Output.JsonIndent);
        return this;
    }

    public override IPipeline? Build(IPipelineWriter? writer = null)
    {
        var filter = new BaselineFilter(_Name);
        return new GetBaselinePipeline(
            pipeline: PrepareContext(PipelineHookActions.Empty, writer ?? PrepareWriter()),
            source: Source,
            filter: filter
        );
    }
}

#nullable restore
