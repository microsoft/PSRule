// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Data;
using PSRule.Emitters;

namespace PSRule.Pipeline.Emitters;

/// <summary>
/// An <seealso cref="IEmitter"/> for processing PowerShell Data.
/// </summary>
internal sealed class PowerShellDataEmitter : FileEmitter
{
    private const string EXTENSION_PSD1 = ".psd1";

    /// <summary>
    /// Accept the file if it is a PowerShell Data file.
    /// </summary>
    protected override bool AcceptsFilePath(IEmitterContext context, IFileInfo info)
    {
        return info != null && info.Extension == EXTENSION_PSD1;
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
        return context.Format == Options.InputFormat.PowerShellData;
    }

    /// <inheritdoc/>
    protected override bool VisitString(IEmitterContext context, string content)
    {
        if (string.IsNullOrEmpty(content)) return false;

        var ast = System.Management.Automation.Language.Parser.ParseInput(content, out _, out _);
        var hashtables = ast.FindAll(item => item is System.Management.Automation.Language.HashtableAst, false);

        var result = new List<PSObject>();
        foreach (var hashtable in hashtables)
        {
            if (hashtable?.Parent?.Parent?.Parent?.Parent == ast)
                result.Add(PSObject.AsPSObject(hashtable.SafeGetValue()));
        }
        var value = result.ToArray();
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
