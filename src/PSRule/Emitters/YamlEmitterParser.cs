// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;
using YamlDotNet.Core;

namespace PSRule.Emitters;

/// <summary>
/// A custom parser that implements source mapping.
/// </summary>
internal sealed class YamlEmitterParser : Parser
{
    public YamlEmitterParser(TextReader input, IFileInfo info)
        : base(input)
    {
        Info = info;
    }

    public IFileInfo Info { get; }
}
