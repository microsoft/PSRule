// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Options
{
    /// <summary>
    /// Configures the hashing algorithm used by the PSRule runtime.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HashAlgorithm
    {
        /// <summary>
        /// Use SHA256.
        /// </summary>
        SHA256,

        /// <summary>
        /// Use SHA384.
        /// </summary>
        SHA384,

        /// <summary>
        /// Use SHA512.
        /// </summary>
        SHA512
    }
}
