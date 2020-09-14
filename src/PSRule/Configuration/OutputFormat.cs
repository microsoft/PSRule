// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Configuration
{
    /// <summary>
    /// The formats to return results in.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OutputFormat : byte
    {
        None = 0,

        Yaml = 1,

        Json = 2,

        NUnit3 = 3,

        Csv = 4,

        Wide = 5,

        Markdown = 6
    }
}
