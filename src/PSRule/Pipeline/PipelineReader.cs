using System;
using System.Collections.Concurrent;
using System.IO;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    internal sealed class PipelineReader
    {
        private readonly VisitTargetObject _Input;
        private readonly string[] _InputPath;
        private readonly ConcurrentQueue<PSObject> _Queue;

        public PipelineReader(VisitTargetObject input, string[] inputPath)
        {
            _Input = input;
            _InputPath = inputPath;
            _Queue = new ConcurrentQueue<PSObject>();
        }

        public int Count
        {
            get { return _Queue.Count; }
        }

        public bool IsEmpty
        {
            get { return _Queue.IsEmpty; }
        }

        public void Enqueue(PSObject sourceObject)
        {
            if (sourceObject == null)
                return;

            if (_Input == null)
            {
                _Queue.Enqueue(sourceObject);
                return;
            }

            // Visit the object, which may change or expand the object
            var input = _Input(sourceObject);

            if (input == null)
                return;

            foreach (var item in input)
            {
                _Queue.Enqueue(item);
            }
        }

        public bool TryDequeue(out PSObject sourceObject)
        {
            return _Queue.TryDequeue(out sourceObject);
        }

        public void Open()
        {
            if (_InputPath != null)
            {
                // Read each file

                foreach (var p in _InputPath)
                {
                    if (p.IsUri())
                    {
                        Enqueue(PSObject.AsPSObject(new Uri(p)));
                    }
                    else
                    {
                        Enqueue(PSObject.AsPSObject(new FileInfo(p)));
                    }
                }
            }
        }
    }
}
