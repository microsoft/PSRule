// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Configuration;

/// <summary>
/// The encoding format to convert output to.
/// <seealso href="https://microsoft.github.io/PSRule/latest/concepts/PSRule/en-US/about_PSRule_Options/#outputencoding"/>
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum OutputEncoding
{
    /// <summary>
    /// UTF-8 with Byte Order Mark (BOM). This is the default.
    /// </summary>
    Default = 0,

    /// <summary>
    /// UTF-8 without Byte Order Mark (BOM).
    /// </summary>
    UTF8,

    /// <summary>
    /// UTF-7.
    /// </summary>
    UTF7,

    /// <summary>
    /// Unicode. Same as UTF-16.
    /// </summary>
    Unicode,

    /// <summary>
    /// UTF-32.
    /// </summary>
    UTF32,

    /// <summary>
    /// ASCII.
    /// </summary>
    ASCII
}
