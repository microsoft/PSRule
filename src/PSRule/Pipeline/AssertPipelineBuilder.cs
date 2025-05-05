// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Pipeline.Output;
using PSRule.Rules;

namespace PSRule.Pipeline;

/// <summary>
/// A helper to construct the pipeline for Assert-PSRule.
/// </summary>
internal sealed class AssertPipelineBuilder : InvokePipelineBuilderBase
{
    private AssertWriter _Writer;

    internal AssertPipelineBuilder(Source[] source, IHostContext hostContext)
        : base(source, hostContext) { }

    protected override PipelineWriter PrepareWriter()
    {
        return GetWriter();
    }

    protected override PipelineWriter GetOutput(bool writeHost = false)
    {
        return base.GetOutput(writeHost: true);
    }

    private AssertWriter GetWriter()
    {
        if (_Writer == null)
        {
            var next = ShouldOutput() ? base.PrepareWriter() : null;
            _Writer = new AssertWriter(
                option: Option,
                source: Source,
                inner: GetOutput(writeHost: true),
                next: next,
                style: Option.Output.Style ?? OutputOption.Default.Style.Value,
                resultVariableName: _ResultVariableName,
                hostContext: HostContext
            );
        }
        return _Writer;
    }

    private bool ShouldOutput()
    {
        return !(string.IsNullOrEmpty(Option.Output.Path) ||
            Option.Output.Format == OutputFormat.Wide ||
            Option.Output.Format == OutputFormat.None);
    }

    public sealed override IPipeline Build(IPipelineWriter writer = null)
    {
        writer ??= PrepareWriter();
        Unblock(writer);
        if (!RequireModules() || !RequireWorkspaceCapabilities() || !RequireSources())
            return null;

        var context = PrepareContext(PipelineHookActions.Default, writer: HandleJobSummary(writer ?? PrepareWriter()), checkModuleCapabilities: true);
        if (context == null)
            return null;

        return new InvokeRulePipeline
        (
            context: context,
            source: Source,
            outcome: RuleOutcome.Processed
        );
    }

    private IPipelineWriter HandleJobSummary(IPipelineWriter writer)
    {
        if (string.IsNullOrEmpty(Option.Output.JobSummaryPath))
            return writer;

        return new JobSummaryWriter
        (
            inner: writer,
            option: Option,
            shouldProcess: ShouldProcess,
            source: Source
        );
    }
}
