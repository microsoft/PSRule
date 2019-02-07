using System;
using System.Collections.Concurrent;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    /// <summary>
    /// The default stream that queues objects for processing in the pipeline.
    /// </summary>
    internal sealed class PipelineStream : IPipelineStream
    {
        private readonly VisitTargetObject _InputVisitor;
        private readonly Action<object, bool> _OutputVisitor;

        private ConcurrentQueue<PSObject> _Queue;

        public PipelineStream(VisitTargetObject input, Action<object, bool> output)
        {
            _InputVisitor = input;
            _OutputVisitor = output;
            _Queue = new ConcurrentQueue<PSObject>();
        }

        public bool IsEmpty
        {
            get
            {
                return _Queue.IsEmpty;
            }
        }

        public void Process(PSObject targetObject)
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

        internal void Output(object o, bool expandCollection)
        {
            _OutputVisitor(o, expandCollection);
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
