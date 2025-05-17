// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;

namespace PSRule.Help;

internal static class MarkdownConvert
{
    private const char Dash = '-';
    private const string TripleDash = "---";

    public static PSObject[] DeserializeObject(string markdown)
    {
        var result = YamlHeader(markdown);
        return result == null ? System.Array.Empty<PSObject>() : new PSObject[] { result };
    }

    private static PSObject? YamlHeader(string markdown)
    {
        var stream = new MarkdownStream(markdown);
        if (stream.EOF || stream.Line > 1 || stream.Current != Dash)
            return null;

        // Check if the line is just dashes indicating start of yaml header
        if (!stream.PeakLine(Dash, out var count) || count < 2)
            return null;

        stream.Skip(count + 1);
        stream.SkipLineEnding();

        var yaml = stream.CaptureUntil(TripleDash, onNewLine: true).Trim();
        var d = new YamlDotNet.Serialization.DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithTypeConverter(new PSObjectYamlTypeConverter())
            .WithNodeTypeResolver(new PSObjectYamlTypeResolver())
            .Build();

        return d.Deserialize<PSObject>(yaml);
    }
}
