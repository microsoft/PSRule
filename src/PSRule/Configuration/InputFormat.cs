// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Configuration;

/// <summary>
/// The formats to convert input from.
/// <seealso href="https://microsoft.github.io/PSRule/latest/concepts/PSRule/en-US/about_PSRule_Options/#inputformat"/>
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum InputFormat
{
    /// <summary>
    /// Treat strings as plain text and do not deserialize files.
    /// </summary>
    None = 0,

    /// <summary>
    /// Deserialize as one or more YAML objects.
    /// </summary>
    Yaml = 1,

    /// <summary>
    /// Deserialize as one or more JSON objects.
    /// </summary>
    Json = 2,

    /// <summary>
    /// Deserialize as a markdown object.
    /// </summary>
    Markdown = 3,

    /// <summary>
    /// Deserialize as a PowerShell data object.
    /// </summary>
    PowerShellData = 4,

    /// <summary>
    /// Files are treated as objects and not deserialized.
    /// </summary>
    File = 5,

    /// <summary>
    /// Detect format based on file extension. This is the default.
    /// </summary>
    Detect = 255
}
