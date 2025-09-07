// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

internal interface IGetLocalizedPathContext
{
    /// <summary>
    /// The current source file.
    /// </summary>
    ISourceFile? Source { get; }

    string? GetLocalizedPath(string file, out string? culture);
}
