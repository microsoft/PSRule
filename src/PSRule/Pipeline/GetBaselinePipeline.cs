using PSRule.Host;
using PSRule.Rules;
using System;

namespace PSRule.Pipeline
{
    public sealed class GetBaselinePipeline : IDisposable
    {
        private readonly StreamManager _StreamManager;
        private readonly PipelineContext _Context;
        private readonly Source[] _Source;
        private readonly IResourceFilter _Filter;

        // Track whether Dispose has been called.
        private bool _Disposed = false;

        internal GetBaselinePipeline(StreamManager streamManager, PipelineContext context, Source[] source, IResourceFilter filter)
        {
            _StreamManager = streamManager;
            _Context = context;
            _Source = source;
            _Filter = filter;
        }

        public void Begin()
        {

        }

        public void Process()
        {
            foreach (var baseline in HostHelper.GetBaseline(_Source, _Context))
            {
                if (_Filter == null || _Filter.Match(baseline))
                    _StreamManager.Output(baseline);
            }
        }

        public void End()
        {

        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
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
