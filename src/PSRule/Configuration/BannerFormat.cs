// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace PSRule.Configuration
{
    /// <summary>
    /// The information displayed for Assert-PSRule banner.
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

        Default = Title | Source | SupportLinks,
        Minimal = Source
    }
}
