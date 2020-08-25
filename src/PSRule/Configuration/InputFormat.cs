// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Configuration
{
    /// <summary>
    /// The formats to convert input from.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum InputFormat : byte
    {
        None = 0,

        Yaml = 1,

        Json = 2,

        Markdown = 3,

        PowerShellData = 4,

        File = 5,

        Detect = 255
    }
}
