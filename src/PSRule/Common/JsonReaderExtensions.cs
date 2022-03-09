// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Definitions;

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
            if (string.IsNullOrEmpty(file) || !TryLineInfo(reader, out int lineNumber, out int linePosition))
                return false;

            extent = new SourceExtent(file, lineNumber, linePosition);
            return true;
        }
    }
}
