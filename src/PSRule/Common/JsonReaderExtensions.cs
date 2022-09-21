// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Newtonsoft.Json;
using PSRule.Definitions;
using PSRule.Pipeline;
using PSRule.Resources;

namespace PSRule
{
    internal static class JsonReaderExtensions
    {
        public static bool TryLineInfo(this JsonReader reader, out int lineNumber, out int linePosition)
        {
            lineNumber = 0;
            linePosition = 0;
            if (!(reader is IJsonLineInfo lineInfo && lineInfo.HasLineInfo()))
                return false;

            lineNumber = lineInfo.LineNumber;
            linePosition = lineInfo.LinePosition;
            return true;
        }

        public static bool GetSourceExtent(this JsonReader reader, string file, out ISourceExtent extent)
        {
            extent = null;
            if (string.IsNullOrEmpty(file) || !TryLineInfo(reader, out var lineNumber, out var linePosition))
                return false;

            extent = new SourceExtent(file, lineNumber, linePosition);
            return true;
        }

        [DebuggerStepThrough]
        public static bool TryConsume(this JsonReader reader, JsonToken token)
        {
            if (reader.TokenType != token)
                return false;

            reader.Read();
            return true;
        }

        [DebuggerStepThrough]
        public static bool TryConsume(this JsonReader reader, JsonToken token, out object value)
        {
            value = null;
            if (reader.TokenType != token)
                return false;

            value = reader.Value;
            reader.Read();
            return true;
        }

        [DebuggerStepThrough]
        public static void Consume(this JsonReader reader, JsonToken token)
        {
            if (reader.TokenType != token)
                throw new PipelineSerializationException(PSRuleResources.ReadJsonFailedExpectedToken, Enum.GetName(typeof(JsonToken), reader.TokenType));

            reader.Read();
        }

        /// <summary>
        /// Skip JSON comments.
        /// </summary>
        [DebuggerStepThrough]
        public static bool SkipComments(this JsonReader reader, out bool hasComments)
        {
            hasComments = false;
            while (reader.TokenType == JsonToken.Comment || reader.TokenType == JsonToken.None)
            {
                if (reader.TokenType == JsonToken.Comment)
                    hasComments = true;

                if (!reader.Read())
                    return false;
            }
            return true;
        }
    }
}
