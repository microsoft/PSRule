// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Configuration;

/// <summary>
/// The information displayed for Assert-PSRule footer.
/// </summary>
[Flags]
[JsonConverter(typeof(StringEnumConverter))]
public enum FooterFormat
{
    /// <summary>
    /// No footer is shown.
    /// </summary>
    None = 0,

    /// <summary>
    /// A summary of rules processed.
    /// </summary>
    RuleCount = 1,

    /// <summary>
    /// Information about the run.
    /// </summary>
    RunInfo = 2,

    /// <summary>
    /// Information about the output file if an output path is set.
    /// </summary>
    OutputFile = 4,

    /// <summary>
    /// The default footer output.
    /// </summary>
    Default = RuleCount | RunInfo | OutputFile
}
