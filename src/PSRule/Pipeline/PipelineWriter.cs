// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Rules;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    internal delegate void WriteOutput(object o, bool enumerate);

    internal abstract class PipelineWriter
    {
        private readonly WriteOutput _Output;

        protected PipelineWriter(WriteOutput output)
        {
            _Output = output;
        }

        public virtual void Write(object o, bool enumerate)
        {
            _Output(o, enumerate);
        }

        public virtual void End()
        {
            // Do nothing
        }
    }

    internal sealed class PassThruWriter : PipelineWriter
    {
        private readonly bool _Wide;

        internal PassThruWriter(WriteOutput output, bool wide)
            : base(output)
        {
            _Wide = wide;
        }

        public override void Write(object o, bool enumerate)
        {
            if (!(InvokeResult(o) || Rule(o)))
                base.Write(o, enumerate);
        }

        private bool InvokeResult(object o)
        {
            if (!(o is InvokeResult result))
                return false;

            var records = result.AsRecord();
            if (_Wide)
                WriteWideObject(records);
            else
                base.Write(records, true);

            return true;
        }

        private bool Rule(object o)
        {
            if (!(o is IEnumerable<Rule> rule))
                return false;

            if (_Wide)
                WriteWideObject(rule);
            else
                base.Write(rule, true);

            return true;
        }

        private void WriteWideObject<T>(IEnumerable<T> collection)
        {
            var typeName = string.Concat(typeof(T).FullName, "+Wide");

            foreach (var item in collection)
            {
                var o = PSObject.AsPSObject(item);
                o.TypeNames.Insert(0, typeName);
                base.Write(o, false);
            }
        }
    }
}
