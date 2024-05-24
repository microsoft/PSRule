// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Data;
using PSRule.Emitters;
using PSRule.Help;

namespace PSRule.Pipeline.Emitters;

/// <summary>
/// An <seealso cref="IEmitter"/> for processing Markdown.
/// </summary>
internal sealed class MarkdownEmitter : FileEmitter
{
    private const string EXTENSION_MARKDOWN = ".markdown";
    private const string EXTENSION_MD = ".md";

    /// <summary>
    /// Accept the file if it is a markdown file.
    /// </summary>
    protected override bool AcceptsFilePath(IEmitterContext context, IFileInfo info)
    {
        return info != null && (info.Extension == EXTENSION_MD || info.Extension == EXTENSION_MARKDOWN);
    }

    /// <inheritdoc/>
    protected override bool VisitFile(IEmitterContext context, IFileStream stream)
    {
        using var reader = stream.AsTextReader();
        if (reader == null) return false;

        var content = reader.ReadToEnd();
        return VisitString(context, content);
    }

    /// <inheritdoc/>
    protected override bool AcceptsString(IEmitterContext context)
    {
        return context.Format == Options.InputFormat.Markdown;
    }

    /// <inheritdoc/>
    protected override bool VisitString(IEmitterContext context, string content)
    {
        if (string.IsNullOrEmpty(content)) return false;

        var value = MarkdownConvert.DeserializeObject(content);
        VisitItems(context, value, null);
        return true;
    }

    private static void VisitItems(IEmitterContext context, IEnumerable<PSObject> items, IFileInfo sourceInfo)
    {
        if (items == null)
            return;

        foreach (var item in items)
        {
            if (item == null)
                return;

            var value = new TargetObject(item);
            NoteSource(value, sourceInfo);

            context.Emit(value);
        }
    }

    private static void NoteSource(TargetObject value, IFileInfo source)
    {
        if (value == null || source == null)
            return;

        //value.Value.UseTargetInfo(out var targetInfo);
        //targetInfo.UpdateSource(source);
        //value.Source.AddRange(targetInfo.Source.ToArray());
        //value.Issue.AddRange(targetInfo.Issue.ToArray());
    }
}
