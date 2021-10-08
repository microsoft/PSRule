// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using Newtonsoft.Json;
using PSRule.Configuration;

namespace PSRule.Pipeline.Output
{
    internal sealed class JsonOutputWriter : SerializationOutputWriter<object>
    {
        internal JsonOutputWriter(PipelineWriter inner, PSRuleOption option)
            : base(inner, option) { }

        protected override string Serialize(object[] o)
        {
            using (StringWriter stringWriter = new StringWriter())
            {
                using (JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    JsonSerializer jsonSerializer = new JsonSerializer();

                    jsonSerializer.NullValueHandling = NullValueHandling.Ignore;

                    int? outputJsonIndent = Option.Output.JsonIndent;
                    if (outputJsonIndent.HasValue && outputJsonIndent > 0)
                    {
                        jsonSerializer.Formatting = Formatting.Indented;
                        jsonTextWriter.Indentation = outputJsonIndent.Value;
                    }

                    jsonSerializer.ContractResolver = new SortedPropertyContractResolver();

                    jsonSerializer.Converters.Add(new ErrorCategoryJsonConverter());

                    jsonSerializer.Serialize(jsonTextWriter, o);

                    return stringWriter.ToString();
                }
            }
        }
    }
}
