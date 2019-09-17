using PSRule.Configuration;
using PSRule.Rules;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    /// <summary>
    /// The default stream that queues objects for processing in the pipeline.
    /// </summary>
    internal sealed class StreamManager : IStreamManager
    {
        private readonly IPipelineStream _Stream;
        private readonly VisitTargetObject _InputVisitor;
        private readonly ConcurrentQueue<PSObject> _Queue;

        internal StreamManager(PSRuleOption option, IPipelineStream stream, VisitTargetObject input)
        {
            _Stream = stream;
            _InputVisitor = input;
            _Queue = new ConcurrentQueue<PSObject>();
            _Stream.Manager = this;
        }

        public bool IsEmpty
        {
            get
            {
                return _Queue.IsEmpty;
            }
        }

        public void Begin()
        {
            _Stream.Begin();
        }

        public void End(IEnumerable<RuleSummaryRecord> summary)
        {
            _Stream.End(summary);
        }

        public void Process(PSObject targetObject)
        {
            _Stream.Process(targetObject);
        }

        void IStreamManager.Process(PSObject targetObject)
        {
            if (targetObject == null)
            {
                return;
            }

            // Visit the object, which may change or expand the object
            var input = _InputVisitor(targetObject);

            if (input == null)
            {
                return;
            }

            foreach (var item in input)
            {
                _Queue.Enqueue(item);
            }
        }

        internal void Output(InvokeResult result)
        {
            _Stream.Output(result);
        }

        internal void Output(Baseline result)
        {
            _Stream.Output(result);
        }

        /// <summary>
        /// Get the next object.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        internal bool Next(out PSObject result)
        {
            result = null;

            if (_Queue.TryDequeue(out PSObject o))
            {
                result = o;
            }

            return result != null;
        }
    }
}
