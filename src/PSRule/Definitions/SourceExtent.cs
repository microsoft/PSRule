// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

internal sealed class SourceExtent : ISourceExtent
{
    internal SourceExtent(string file, int? line)
        : this(file, line, null)
    {
        File = file;
        Line = line;
    }

    internal SourceExtent(string file, int? line, int? position)
    {
        File = file;
        Line = line;
        Position = position;
    }

    public string File { get; }

    public int? Line { get; }

    public int? Position { get; }
}
