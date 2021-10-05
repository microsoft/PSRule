// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Configuration
{
    /// <summary>
    /// The indentation level for JSON output
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OutputJsonIndent
    {
        /// <summary>
        /// Machine first compact indentation
        /// </summary>
        MachineFirst = 0,

        /// <summary>
        /// Indent with 2 spaces
        /// </summary>
        TwoSpaces = 2,

        /// <summary>
        /// Indent with 4 spaces
        /// </summary>
        FourSpaces = 4
    }
}