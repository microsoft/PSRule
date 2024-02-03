// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace PSRule;

internal sealed class JsonCommentWriter : JsonTextWriter
{
    public JsonCommentWriter(TextWriter textWriter)
        : base(textWriter) { }

    public override void WriteComment(string text)
    {
        SetWriteState(JsonToken.Comment, text);
        if (Indentation > 0 && Formatting == Formatting.Indented)
            WriteIndent();
        else
            WriteRaw(System.Environment.NewLine);

        WriteRaw("// ");
        WriteRaw(text);
        if (Indentation == 0 || Formatting == Formatting.None)
            WriteRaw(System.Environment.NewLine);
    }
}
