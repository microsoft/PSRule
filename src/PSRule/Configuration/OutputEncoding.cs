// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Configuration
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OutputEncoding
    {
        Default = 0,

        UTF8,

        UTF7,

        Unicode,

        UTF32,

        ASCII
    }
}
