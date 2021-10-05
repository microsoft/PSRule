// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Configuration;
using System.IO;

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

                    OutputJsonIndent? outputJsonIndent = Option.Output.JsonIndent;
                    if (outputJsonIndent.HasValue && outputJsonIndent != OutputJsonIndent.MachineFirst)
                    {
                        jsonSerializer.Formatting = Formatting.Indented;
                        jsonTextWriter.Indentation = (int)outputJsonIndent;
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
