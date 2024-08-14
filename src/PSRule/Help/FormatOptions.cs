// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Help;

/// <summary>
/// Define options that determine how markdown will be rendered.
/// </summary>
[Flags()]
internal enum FormatOptions
{
    None = 0,

    /// <summary>
    /// Add a line break after headers.
    /// </summary>
    LineBreak = 1
}
