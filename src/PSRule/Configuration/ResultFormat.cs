﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Configuration
{
    /// <summary>
    /// The format to return to the pipeline after executing rules.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResultFormat : byte
    {
        Detail = 1,

        Summary = 2
    }
}
