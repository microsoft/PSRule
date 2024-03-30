// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Options;

namespace PSRule.Pipeline;

/// <summary>
/// A helper to build a pipeline to return target objects.
/// </summary>
public interface IGetTargetPipelineBuilder : IPipelineBuilder
{
    /// <summary>
    /// Specifies a path for reading input objects from disk.
    /// </summary>
    void InputPath(string[] path);
}

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
        return new GetTargetPipeline(PrepareContext(null, null, null), PrepareReader(), writer ?? PrepareWriter());
    }

    /// <inheritdoc/>
    protected override PipelineInputStream PrepareReader()
    {
        if (!string.IsNullOrEmpty(Option.Input.ObjectPath))
        {
            AddVisitTargetObjectAction((sourceObject, next) =>
            {
                return PipelineReceiverActions.ReadObjectPath(sourceObject, next, Option.Input.ObjectPath, true);
            });
        }

        if (Option.Input.Format == InputFormat.Yaml)
        {
            AddVisitTargetObjectAction((sourceObject, next) =>
            {
                return PipelineReceiverActions.ConvertFromYaml(sourceObject, next);
            });
        }
        else if (Option.Input.Format == InputFormat.Json)
        {
            AddVisitTargetObjectAction((sourceObject, next) =>
            {
                return PipelineReceiverActions.ConvertFromJson(sourceObject, next);
            });
        }
        else if (Option.Input.Format == InputFormat.Markdown)
        {
            AddVisitTargetObjectAction((sourceObject, next) =>
            {
                return PipelineReceiverActions.ConvertFromMarkdown(sourceObject, next);
            });
        }
        else if (Option.Input.Format == InputFormat.PowerShellData)
        {
            AddVisitTargetObjectAction((sourceObject, next) =>
            {
                return PipelineReceiverActions.ConvertFromPowerShellData(sourceObject, next);
            });
        }
        else if (Option.Input.Format == InputFormat.File)
        {
            AddVisitTargetObjectAction((sourceObject, next) =>
            {
                return PipelineReceiverActions.ConvertFromGitHead(sourceObject, next);
            });
        }
        else if (Option.Input.Format == InputFormat.Detect && _InputPath != null)
        {
            AddVisitTargetObjectAction((sourceObject, next) =>
            {
                return PipelineReceiverActions.DetectInputFormat(sourceObject, next);
            });
        }
        return new PipelineInputStream(VisitTargetObject, _InputPath, GetInputObjectSourceFilter());
    }
}

/// <summary>
/// A pipeline that gets target objects through the pipeline.
/// </summary>
internal sealed class GetTargetPipeline : RulePipeline
{
    internal GetTargetPipeline(PipelineContext context, PipelineInputStream reader, IPipelineWriter writer)
        : base(context, null, reader, writer) { }

    public override void Process(PSObject sourceObject)
    {
        try
        {
            Reader.Enqueue(sourceObject);
            while (Reader.TryDequeue(out var next))
                Writer.WriteObject(next.Value, false);
        }
        catch (Exception)
        {
            End();
            throw;
        }
    }
}
