// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Options;
using PSRule.Runtime;

namespace PSRule.Pipeline;

internal abstract class RulePipeline : IPipeline
{
    protected readonly PipelineContext Pipeline;
    protected readonly RunspaceContext Context;
    protected readonly Source[] Source;
    protected readonly IPipelineReader Reader;
    protected readonly IPipelineWriter Writer;

    // Track whether Dispose has been called.
    private bool _Disposed;

    protected RulePipeline(PipelineContext pipelineContext, Source[] source, IPipelineReader reader, IPipelineWriter writer)
    {
        Result = new DefaultPipelineResult(writer, pipelineContext.Option.Execution.Break.GetValueOrDefault(ExecutionOption.Default.Break.Value));
        Pipeline = pipelineContext;
        Context = new RunspaceContext(Pipeline);
        Source = source;
        Reader = reader;
        Writer = writer;

        // Initialize contexts
        Context.Init(source);
    }

    #region IPipeline

    /// <inheritdoc/>
    IPipelineResult IPipeline.Result => Result;

    /// <inheritdoc/>
    public DefaultPipelineResult Result { get; }

    /// <inheritdoc/>
    public virtual void Begin()
    {
        Writer.Begin();
        Context.Begin();
        Reader.Open();
    }

    /// <inheritdoc/>
    public virtual void Process(PSObject sourceObject)
    {
        // Do nothing
    }

    /// <inheritdoc/>
    public virtual void End()
    {
        Writer.End(Result);
    }

    #endregion IPipeline

    #region IDisposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                Writer.Dispose();
                Context.Dispose();
                Pipeline.Dispose();
            }
            _Disposed = true;
        }
    }

    #endregion IDisposable
}
