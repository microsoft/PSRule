// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PSRule.Configuration;
using PSRule.Definitions.Baselines;

namespace PSRule.Pipeline.Output
{
    internal sealed class JsonOutputWriter : SerializationOutputWriter<object>
    {
        internal JsonOutputWriter(PipelineWriter inner, PSRuleOption option)
            : base(inner, option) { }

        protected override string Serialize(object[] o)
        {
            using var stringWriter = new StringWriter();
            using var jsonTextWriter = new JsonCommentWriter(stringWriter);

            var jsonSerializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var outputJsonIndent = Option.Output.JsonIndent ?? 0;

            if (outputJsonIndent > 0)
            {
                jsonSerializer.Formatting = Formatting.Indented;
                jsonTextWriter.Indentation = outputJsonIndent;
            }

            jsonSerializer.ContractResolver = new OrderedPropertiesContractResolver();

            jsonSerializer.Converters.Add(new ErrorCategoryJsonConverter());
            jsonSerializer.Converters.Add(new PSObjectJsonConverter());

            // To avoid writing baselines with an extra outer array
            // We can serialize the first object which has all the baselines
            if (o[0] is IEnumerable<Baseline> baselines)
            {
                jsonSerializer.Converters.Add(new BaselineConverter());
                jsonSerializer.Serialize(jsonTextWriter, baselines);
            }
            else
            {
                jsonSerializer.Serialize(jsonTextWriter, o);
            }

            return stringWriter.ToString();
        }
    }
}
