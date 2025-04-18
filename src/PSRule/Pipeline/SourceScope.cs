// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using PSRule.Definitions;

namespace PSRule.Pipeline;

#nullable enable

internal sealed class SourceScope(ISourceFile source)
{
    public readonly ISourceFile File = source;

    public string[] SourceContentCache
    {
        get
        {
            return System.IO.File.ReadAllLines(File.Path, Encoding.UTF8);
        }
    }
}

#nullable restore
