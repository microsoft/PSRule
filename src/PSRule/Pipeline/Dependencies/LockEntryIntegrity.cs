// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel.DataAnnotations;

namespace PSRule.Pipeline.Dependencies;

#nullable enable

/// <summary>
/// Split out an integrity hash string into the algorithm and hash value.
/// </summary>
public sealed class LockEntryIntegrity(IntegrityAlgorithm algorithm, string hash)
{
    /// <summary>
    /// The algorithm used to generate the hash.
    /// </summary>
    [Required]
    public IntegrityAlgorithm Algorithm { get; set; } = algorithm;

    /// <summary>
    /// The base64 encoded hash value.
    /// </summary>
    [Required]
    public string Hash { get; set; } = hash;
}

#nullable restore
