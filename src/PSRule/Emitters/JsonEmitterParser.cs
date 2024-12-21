// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Data;

namespace PSRule.Emitters;

internal sealed class JsonEmitterParser : JsonTextReader
{
    public JsonEmitterParser(TextReader reader, IFileInfo info)
        : base(reader)
    {
        Info = info;
    }

    public IFileInfo Info { get; }
}
