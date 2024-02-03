// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Configuration;

/// <summary>
/// The formats to return results in.
/// <seealso href="https://microsoft.github.io/PSRule/latest/concepts/PSRule/en-US/about_PSRule_Options/#outputformat"/>
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum OutputFormat
{
    /// <summary>
    /// Output is presented as an object using PowerShell defaults. This is the default.
    /// </summary>
    None = 0,

    /// <summary>
    /// Output is serialized as YAML.
    /// </summary>
    Yaml = 1,

    /// <summary>
    /// Output is serialized as JSON.
    /// </summary>
    Json = 2,

    /// <summary>
    /// Output is serialized as NUnit3 (XML).
    /// </summary>
    NUnit3 = 3,

    /// <summary>
    /// Output is serialized as a comma-separated values (CSV).
    /// </summary>
    Csv = 4,

    /// <summary>
    /// Output is presented using the wide table format, which includes reason and wraps columns.
    /// </summary>
    Wide = 5,

    /// <summary>
    /// Output is serialized as Markdown.
    /// </summary>
    Markdown = 6,

    /// <summary>
    /// Output is serialized as SARIF.
    /// </summary>
    Sarif = 7
}
