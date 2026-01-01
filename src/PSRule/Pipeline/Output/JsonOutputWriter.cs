// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PSRule.Configuration;
using PSRule.Converters.Json;
using PSRule.Definitions.Baselines;

namespace PSRule.Pipeline.Output;

internal sealed class JsonOutputWriter : SerializationOutputWriter<object>
{
    private const string EMPTY_ARRAY = "[]";

    internal JsonOutputWriter(PipelineWriter inner, PSRuleOption option, ShouldProcess shouldProcess)
        : base(inner, option, shouldProcess) { }

    protected override string Serialize(object[] o)
    {
        return ToJson(o, Option.Output.JsonIndent);
    }

    internal static string ToJson(object[] o, int? jsonIndent)
    {
        if (o == null || o.Length == 0)
            return EMPTY_ARRAY;

        using var stringWriter = new StringWriter();
        using var jsonTextWriter = new JsonCommentWriter(stringWriter);

        var jsonSerializer = new JsonSerializer
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        var outputJsonIndent = jsonIndent ?? 0;
        if (outputJsonIndent > 0)
        {
            jsonSerializer.Formatting = Formatting.Indented;
            jsonTextWriter.Indentation = outputJsonIndent;
        }

        jsonSerializer.ContractResolver = new OrderedPropertiesContractResolver();
        jsonSerializer.Converters.Add(new ErrorCategoryJsonConverter());
        jsonSerializer.Converters.Add(new PSObjectJsonConverter());
        jsonSerializer.Converters.Add(new StringEnumConverter());
        jsonSerializer.Converters.Add(new ResourceIdJsonConverter());
        jsonSerializer.Converters.Add(new ResourceIdReferenceJsonConverter());

        // To avoid writing baselines with an extra outer array
        // We can serialize the first object which has all the baselines
        if (o[0] is IEnumerable<Baseline> baselines)
        {
            jsonSerializer.Converters.Add(new BaselineJsonConverter());
            jsonSerializer.Serialize(jsonTextWriter, baselines);
        }
        else
        {
            jsonSerializer.Serialize(jsonTextWriter, o);
        }

        return stringWriter.ToString();
    }
}
