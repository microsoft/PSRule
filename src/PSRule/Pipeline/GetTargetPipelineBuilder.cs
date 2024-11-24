// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;

namespace PSRule.Pipeline;

/// <summary>
/// A helper to construct the pipeline for Assert-PSRule.
/// </summary>
internal sealed class GetTargetPipelineBuilder : PipelineBuilderBase, IGetTargetPipelineBuilder
{
    private InputPathBuilder _InputPath;

    internal GetTargetPipelineBuilder(Source[] source, IHostContext hostContext)
        : base(source, hostContext)
    {
        _InputPath = null;
    }

    /// <inheritdoc/>
    public override IPipelineBuilder Configure(PSRuleOption option)
    {
        if (option == null)
            return this;

        base.Configure(option);

        Option.Output = new OutputOption();
        Option.Output.Culture = GetCulture(option.Output.Culture);

        ConfigureBinding(option);
        Option.Requires = new RequiresOption(option.Requires);

        return this;
    }

    /// <inheritdoc/>
    public void InputPath(string[] path)
    {
        if (path == null || path.Length == 0)
            return;

        PathFilter required = null;
        if (TryChangedFiles(out var files))
        {
            required = PathFilter.Create(Environment.GetWorkingPath(), path);
            path = files;
        }

        var builder = new InputPathBuilder(GetOutput(), Environment.GetWorkingPath(), "*", GetInputFilter(), required);
        builder.Add(path);
        _InputPath = builder;
    }

    /// <inheritdoc/>
    public override IPipeline Build(IPipelineWriter writer = null)
    {
        return new GetTargetPipeline(PrepareContext(PipelineHookActions.Empty), PrepareReader(), writer ?? PrepareWriter());
    }

    /// <inheritdoc/>
    protected override PipelineInputStream PrepareReader()
    {
        return new PipelineInputStream(GetLanguageScopeSet(), _InputPath, GetInputObjectSourceFilter(), Option);
    }
}
