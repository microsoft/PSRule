// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var isIndented = outputJsonIndent > 0;

            if (isIndented)
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

                var indentMultiplier = string.Concat(Enumerable.Repeat(" ", outputJsonIndent * 2));

                var jsonString = stringWriter.ToString();

                // Workaround given JSON.NET doesn't support writing inline comments
                // Can write multiline comments first then replace with inline comment delimiter
                // We use /\*(.*?)\*/ to capture comments between /* */ deimiters
                //var jsonWithMultilineCommentsReplaced = Regex.Replace(jsonString, @"/\*(.*?)\*/", match =>
                //{
                //    var group = match.Groups[1];
                //    var inlineDelimiterComment = $"// {group.Value.Trim()}";

                //    // If indented prepend newline with correct indentation
                //    // Otherwise append a space for no indentation(machine first)
                //    return isIndented ?
                //        Environment.NewLine + indentMultiplier + inlineDelimiterComment :
                //        inlineDelimiterComment + " ";

                //}, RegexOptions.Singleline);

                return jsonString;
            }

            jsonSerializer.Serialize(jsonTextWriter, o);

            return stringWriter.ToString();
        }
    }
}
