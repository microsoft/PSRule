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
    /// Configure if the format is enabled.
    /// </summary>
    [DefaultValue(null)]
    public bool? Enabled { get; set; }

    /// <summary>
    /// Configures the types to process as this format.
    /// </summary>
    [DefaultValue(null)]
    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
    public string[]? Type { get; set; }

    /// <summary>
    /// Configures a list of replacement tokens to be used by the format.
    /// </summary>
    [DefaultValue(null)]
    public ReplacementStrings? Replace { get; set; }

    /// <summary>
    /// Merge two format type instances by replacing any unset properties from <paramref name="o1"/> with <paramref name="o2"/> values.
    /// Values from <paramref name="o1"/> that are set are not overridden.
    /// </summary>
    public static FormatType Combine(FormatType o1, FormatType o2)
    {
        return new FormatType
        {
            Enabled = o1.Enabled ?? o2.Enabled,
            Type = o1.Type ?? o2.Type,
            Replace = o1.Replace ?? o2.Replace
        };
    }
}
