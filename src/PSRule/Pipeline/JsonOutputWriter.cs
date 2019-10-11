// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Rules;
using System.Collections.Generic;

namespace PSRule.Pipeline
{
    internal sealed class JsonOutputWriter : PipelineWriter
    {
        private readonly List<object> _Result;

        public JsonOutputWriter(WriteOutput output) : base(output)
        {
            _Result = new List<object>();
        }

        public override void Write(object o, bool enumerate)
        {
            if (o is InvokeResult result)
            {
                _Result.AddRange(result.AsRecord());
                return;
            }
            if (o is IEnumerable<Rule> rule)
            {
                _Result.AddRange(rule);
                return;
            }
            _Result.Add(o);
        }

        public override void End()
        {
            WriteObjectJson();
        }

        private void WriteObjectJson()
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            var json = JsonConvert.SerializeObject(_Result.ToArray(), settings: settings);
            base.Write(json, false);
        }
    }
}
