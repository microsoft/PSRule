// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using PSRule.Converters.Json;
using PSRule.Data;

namespace PSRule.Pipeline.Dependencies;

#nullable enable

/// <summary>
/// An entry within the lock file.
/// </summary>
public sealed class LockEntry(SemanticVersion.Version version)
{
    /// <summary>
    /// The version to use.
    /// </summary>
    [Required]
    [JsonProperty("version", NullValueHandling = NullValueHandling.Include, Order = 0)]
    public SemanticVersion.Version Version { get; set; } = version;

    /// <summary>
    /// The integrity hash for the module.
    /// </summary>
    // [Required]
    [JsonProperty("integrity", NullValueHandling = NullValueHandling.Ignore, Order = 1)]
    [JsonConverter(typeof(LockEntryIntegrityJsonConverter))]
    public LockEntryIntegrity? Integrity { get; set; }

    /// <summary>
    /// Accept pre-release versions in addition to stable module versions.
    /// </summary>
    [DefaultValue(null), JsonProperty("includePrerelease", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IncludePrerelease { get; set; }
}

#nullable restore
