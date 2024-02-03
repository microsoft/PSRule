// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// A source location for a PSRule expression.
/// </summary>
public interface ISourceExtent
{
    /// <summary>
    /// The source file path.
    /// </summary>
    string File { get; }

    /// <summary>
    /// The first line of the expression.
    /// </summary>
    int? Line { get; }

    /// <summary>
    /// The first position of the expression.
    /// </summary>
    int? Position { get; }
}

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
