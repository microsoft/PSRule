// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;
using System;
using System.Collections.Concurrent;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    internal sealed class PipelineReader
    {
        private readonly VisitTargetObject _Input;
        private readonly InputFileInfo[] _InputPath;
        private readonly ConcurrentQueue<PSObject> _Queue;

        public PipelineReader(VisitTargetObject input, InputFileInfo[] inputPath)
        {
            _Input = input;
            _InputPath = inputPath;
            _Queue = new ConcurrentQueue<PSObject>();
        }

        public int Count => _Queue.Count;

        public bool IsEmpty => _Queue.IsEmpty;

        public void Enqueue(PSObject sourceObject, bool skipExpansion = false)
        {
            if (sourceObject == null)
                return;

            if (_Input == null || skipExpansion)
            {
                sourceObject.ConvertTargetInfoProperty();
                _Queue.Enqueue(sourceObject);
                return;
            }

            // Visit the object, which may change or expand the object
            var input = _Input(sourceObject);
            if (input == null)
                return;

            foreach (var item in input)
            {
                sourceObject.ConvertTargetInfoProperty();
                _Queue.Enqueue(item);
            }
        }

        public bool TryDequeue(out PSObject sourceObject)
        {
            return _Queue.TryDequeue(out sourceObject);
        }

        public void Open()
        {
            if (_InputPath == null || _InputPath.Length == 0)
                return;

            // Read each file
            for (var i = 0; i < _InputPath.Length; i++)
            {
                if (_InputPath[i].IsUrl)
                {
                    Enqueue(PSObject.AsPSObject(new Uri(_InputPath[i].FullName)));
                }
                else
                {
                    Enqueue(PSObject.AsPSObject(_InputPath[i]));
                }
            }
        }
    }
}
