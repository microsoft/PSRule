// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using YamlDotNet.Serialization;

namespace PSRule.Options;

/// <summary>
/// A format type.
/// </summary>
public sealed class FormatType
{
    /// <summary>
    /// Configures the types to process as this format.
    /// </summary>
    [DefaultValue(null)]
    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
    public string[]? Type { get; set; }
}
