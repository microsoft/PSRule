using PSRule.Rules;
using System;

namespace PSRule.Pipeline
{
    public abstract class RulePipeline : IDisposable
    {
        internal readonly PipelineContext _Context;
        protected readonly Source[] _Source;

        // Track whether Dispose has been called.
        private bool _Disposed = false;

        internal RulePipeline(PipelineContext context, Source[] source)
        {
            _Context = context;
            _Source = source;
        }

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
                    _Context.Dispose();
                }
                _Disposed = true;
            }
        }

        #endregion IDisposable
    }
}
