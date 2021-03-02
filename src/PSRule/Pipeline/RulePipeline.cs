// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Runtime;
using System;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    internal abstract class RulePipeline : IDisposable, IPipeline
    {
        protected readonly PipelineContext Pipeline;
        protected readonly RunspaceContext Context;
        protected readonly Source[] Source;
        protected readonly PipelineReader Reader;
        protected readonly PipelineWriter Writer;

        // Track whether Dispose has been called.
        private bool _Disposed;

        protected RulePipeline(PipelineContext context, Source[] source, PipelineReader reader, PipelineWriter writer)
        {
            Pipeline = context;
            Context = new RunspaceContext(Pipeline, writer);
            Source = source;
            Reader = reader;
            Writer = writer;
        }

        #region IPipeline

        public virtual void Begin()
        {
            Reader.Open();
            Writer.Begin();
            Context.Begin();
        }

        public virtual void Process(PSObject sourceObject)
        {
            // Do nothing
        }

        public virtual void End()
        {
            Writer.End();
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
}
