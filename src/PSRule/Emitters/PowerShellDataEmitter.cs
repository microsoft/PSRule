// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Immutable;
using System.Management.Automation;
using PSRule.Data;
using PSRule.Pipeline;

namespace PSRule.Emitters;

#nullable enable

/// <summary>
/// An <seealso cref="IEmitter"/> for processing PowerShell Data.
/// </summary>
[EmitterFormat(FORMAT)]
internal sealed class PowerShellDataEmitter : FileEmitter
{
    private const string FORMAT = "powershell_data";

    private static readonly string[] _DefaultTypes = [".psd1"];

    private readonly ImmutableHashSet<string> _Types;
    private readonly KeyValuePair<string, string>[]? _ReplacementTokens;

    public PowerShellDataEmitter(IEmitterConfiguration emitterConfiguration)
    {
        if (emitterConfiguration == null) throw new ArgumentNullException(nameof(emitterConfiguration));

        _Types = emitterConfiguration.GetFormatTypes(FORMAT, _DefaultTypes)!.ToImmutableHashSet();
        _ReplacementTokens = emitterConfiguration.GetFormatReplacementTokens(FORMAT);
    }

    /// <summary>
    /// Accept the file if it is a PowerShell Data file.
    /// </summary>
    protected override bool AcceptsFilePath(IEmitterContext context, IFileInfo info)
    {
        return info != null && info.Extension != null && _Types.Contains(info.Extension);
    }

    /// <inheritdoc/>
    protected override bool VisitFile(IEmitterContext context, IFileStream stream)
    {
        using var reader = GetReader(stream);
        if (reader == null) return false;

        var content = reader.ReadToEnd();
        return VisitString(context, content);
    }

    /// <inheritdoc/>
    protected override bool AcceptsString(IEmitterContext context)
    {
        return string.Equals(context.StringFormat, FORMAT, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    protected override bool VisitString(IEmitterContext context, string content)
    {
        if (string.IsNullOrEmpty(content)) return false;

        var ast = System.Management.Automation.Language.Parser.ParseInput(content, out _, out _);
        var hashtableAst = ast.FindAll(item => item is System.Management.Automation.Language.HashtableAst, false);

        var result = new List<PSObject>();
        foreach (var hashtable in hashtableAst)
        {
            if (hashtable?.Parent?.Parent?.Parent?.Parent == ast)
                result.Add(PSObject.AsPSObject(hashtable.SafeGetValue()));
        }
        var value = result.ToArray();
        VisitItems(context, value, null);
        return true;
    }

    private static void VisitItems(IEmitterContext context, IEnumerable<PSObject> items, IFileInfo? sourceInfo)
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

    private static void NoteSource(TargetObject value, IFileInfo? source)
    {
        if (value == null || source == null)
            return;

        //value.Value.UseTargetInfo(out var targetInfo);
        //targetInfo.UpdateSource(source);
        //value.Source.AddRange(targetInfo.Source.ToArray());
        //value.Issue.AddRange(targetInfo.Issue.ToArray());
    }

    private TextReader GetReader(IFileStream stream)
    {
        return _ReplacementTokens == null || _ReplacementTokens.Length == 0
            ? stream.AsTextReader()
            : new ReplaceTextReader(stream.AsTextReader(), _ReplacementTokens);
    }
}

#nullable restore
