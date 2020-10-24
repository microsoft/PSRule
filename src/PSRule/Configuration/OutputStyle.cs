// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Configuration
{
    /// <summary>
    /// The style to present assert output in.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OutputStyle
    {
        /// <summary>
        /// Formatted text written to host.
        /// </summary>
        Client = 0,

        /// <summary>
        /// Plain text written to output stream.
        /// </summary>
        Plain = 1,

        /// <summary>
        /// Text written to output stream, with fails marked for Azure Pipelines.
        /// </summary>
        AzurePipelines = 2,

        /// <summary>
        /// Text written to output stream, with fails marked for GitHub Actions.
        /// </summary>
        GitHubActions = 3
    }
}
