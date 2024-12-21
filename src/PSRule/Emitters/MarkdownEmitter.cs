// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Immutable;
using System.Management.Automation;
using PSRule.Data;
using PSRule.Help;
using PSRule.Pipeline;

namespace PSRule.Emitters;

/// <summary>
/// An <seealso cref="IEmitter"/> for processing Markdown.
/// </summary>
internal sealed class MarkdownEmitter : FileEmitter
{
    private const string FORMAT = "markdown";

    private static readonly string[] _DefaultTypes = [".md", ".markdown"];

    private readonly ImmutableHashSet<string> _Types;

    public MarkdownEmitter(IEmitterConfiguration emitterConfiguration)
    {
        if (emitterConfiguration == null) throw new ArgumentNullException(nameof(emitterConfiguration));

        _Types = emitterConfiguration.GetFormatTypes(FORMAT, _DefaultTypes).ToImmutableHashSet();
    }

    /// <summary>
    /// Accept the file if it is a markdown file.
    /// </summary>
    protected override bool AcceptsFilePath(IEmitterContext context, IFileInfo info)
    {
        return info != null && _Types.Contains(info.Extension);
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
