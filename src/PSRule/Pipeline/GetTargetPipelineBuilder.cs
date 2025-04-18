// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;

namespace PSRule.Pipeline;

#nullable enable

/// <summary>
/// A helper to construct the pipeline for Assert-PSRule.
/// </summary>
internal sealed class GetTargetPipelineBuilder : PipelineBuilderBase, IGetTargetPipelineBuilder
{
    private InputPathBuilder? _InputPath;

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
    public void InputPath(string[]? path)
    {
        if (path == null || path.Length == 0)
            return;

        var basePath = Environment.GetWorkingPath();
        var filter = GetInputFilter();

        // Wrap with a filter that only allows files that have changed.
        if (TryChangedFiles(out var files) && files != null)
        {
            filter = new ChangedFilesPathFilter(filter, basePath, files);
        }

        var builder = new InputPathBuilder(PrepareWriter(), basePath, "*", filter, null);
        builder.Add(path);
        _InputPath = builder;
    }

    /// <inheritdoc/>
    public void Formats(string[]? format)
    {
        if (format == null || format.Length == 0)
            return;

        EnableFormatsByName(format);
    }

    /// <inheritdoc/>
    public override IPipeline? Build(IPipelineWriter? writer = null)
    {
        var context = PrepareContext(PipelineHookActions.Empty, writer ?? PrepareWriter());
        return context == null ? null : new GetTargetPipeline(context);
    }

    /// <inheritdoc/>
    protected override PipelineInputStream PrepareReader()
    {
        return new PipelineInputStream(GetLanguageScopeSet(), _InputPath, GetInputObjectSourceFilter(), Option, _Output);
    }
}

#nullable restore
