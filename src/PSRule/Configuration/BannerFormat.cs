// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Configuration;

/// <summary>
/// The information displayed for Assert-PSRule banner.
/// See <seealso href="https://microsoft.github.io/PSRule/v2/concepts/PSRule/en-US/about_PSRule_Options/#outputbanner">help</seealso>.
/// </summary>
[Flags]
[JsonConverter(typeof(StringEnumConverter))]
public enum BannerFormat
{
    /// <summary>
    /// No banner is shown.
    /// </summary>
    None = 0,

    /// <summary>
    /// The PSRule title ASCII text is shown.
    /// </summary>
    Title = 1,

    /// <summary>
    /// The rules module versions used in this run are shown.
    /// </summary>
    Source = 2,

    /// <summary>
    /// Supporting links for PSRule and rules modules are shown.
    /// </summary>
    SupportLinks = 4,

    /// <summary>
    /// Information about the repository where PSRule is being run from.
    /// </summary>
    RepositoryInfo = 8,

    /// <summary>
    /// The default information shown in the assert banner.
    /// </summary>
    Default = Title | Source | SupportLinks | RepositoryInfo,

    /// <summary>
    /// A minimal set of information shown in the assert banner.
    /// </summary>
    Minimal = Source
}
