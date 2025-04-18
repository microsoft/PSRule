// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Options;
using PSRule.Runtime;

namespace PSRule.Pipeline;

internal abstract class RulePipeline : IPipeline
{
    protected readonly PipelineContext Pipeline;
    internal readonly LegacyRunspaceContext Context;
    protected readonly Source[] Source;

    // Track whether Dispose has been called.
    private bool _Disposed;

    protected RulePipeline(PipelineContext pipelineContext, Source[] source)
    {
        Result = new DefaultPipelineResult(pipelineContext.Writer, pipelineContext.Option.Execution.Break.GetValueOrDefault(ExecutionOption.Default.Break.Value));
        Pipeline = pipelineContext;
        Context = new LegacyRunspaceContext(Pipeline);
        Source = source;
        // Initialize contexts
        Context.Initialize(source);
    }

    #region IPipeline

    /// <inheritdoc/>
    IPipelineResult IPipeline.Result => Result;

    /// <inheritdoc/>
    public DefaultPipelineResult Result { get; }

    /// <inheritdoc/>
    public virtual void Begin()
    {
        Pipeline.Writer.Begin();
        Context.Begin();
        Pipeline.Reader.Open();
    }

    /// <inheritdoc/>
    public virtual void Process(PSObject sourceObject)
    {
        // Do nothing
    }

    /// <inheritdoc/>
    public virtual void End()
    {
        Pipeline.Writer.End(Result);
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
                Context.Dispose();
                Pipeline.Dispose();
            }
            _Disposed = true;
        }
    }

    #endregion IDisposable
}
