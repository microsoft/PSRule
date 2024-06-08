// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Help;

[DebuggerDisplay("StartPos = (L: {Start}, C: {Column}), EndPos = (L: {End}, C: {Column.End}), Text = {Text}")]
internal sealed class SourceExtent
{
    private readonly string _Source;

    // Lazily cache extracted text
    private string _Text;

    internal SourceExtent(string source, string path, int start, int end, int line, int column)
    {
        _Text = null;
        _Source = source;
        Path = path;
        Start = start;
        End = end;
        Line = line;
        Column = column;
    }

    public readonly string Path;

    public readonly int Start;

    public readonly int End;

    public readonly int Line;

    public readonly int Column;

    public string Text
    {
        get
        {
            if (_Text == null)
            {
                _Text = _Source.Substring(Start, (End - Start));
            }

            return _Text;
        }
    }
}
