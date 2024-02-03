// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Configuration;

/// <summary>
/// The information displayed for job summaries.
/// Currently this is not exposed as a configuration option.
/// </summary>
[Flags]
[JsonConverter(typeof(StringEnumConverter))]
internal enum JobSummaryFormat
{
    /// <summary>
    /// No job summary is outputted.
    /// </summary>
    None = 0,

    /// <summary>
    /// Include rule analysis within job summary.
    /// </summary>
    Analysis = 1,

    /// <summary>
    /// The rules module versions used in this run are shown.
    /// </summary>
    Source = 2,

    /// <summary>
    /// The default information shown in job summaries.
    /// </summary>
    Default = Analysis | Source,
}
