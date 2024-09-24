// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using Newtonsoft.Json;
using PSRule.Data;

namespace PSRule.Pipeline.Dependencies;

/// <summary>
/// An entry within the lock file.
/// </summary>
public sealed class LockEntry
{
    /// <summary>
    /// The version to use.
    /// </summary>
    [JsonProperty("version", NullValueHandling = NullValueHandling.Include)]
    public SemanticVersion.Version Version { get; set; }

    /// <summary>
    /// Accept pre-release versions in addition to stable module versions.
    /// </summary>
    [DefaultValue(null), JsonProperty("includePrerelease", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IncludePrerelease { get; set; }
}
