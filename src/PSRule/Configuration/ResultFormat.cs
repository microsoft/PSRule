// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Configuration
{
    /// <summary>
    /// The format to return to the pipeline after executing rules.
    /// See <seealso href="https://microsoft.github.io/PSRule/v2/concepts/PSRule/en-US/about_PSRule_Options/#outputas">help</seealso>.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResultFormat
    {
        /// <summary>
        /// Return a record per rule per object.
        /// </summary>
        Detail = 1,

        /// <summary>
        /// Return summary results.
        /// </summary>
        Summary = 2
    }
}
