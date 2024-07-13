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
