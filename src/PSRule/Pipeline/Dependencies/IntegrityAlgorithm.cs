// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;

namespace PSRule.Pipeline.Dependencies;

/// <summary>
/// The algorithm used to generate the integrity hash.
/// </summary>
public enum IntegrityAlgorithm
{
    /// <summary>
    /// Unknown algorithm.
    /// </summary>
    [Description("unknown")]
    Unknown = 0,

    /// <summary>
    /// SHA-256 algorithm.
    /// </summary>
    [Description("sha256")]
    SHA256 = 1,

    /// <summary>
    /// SHA-384 algorithm.
    /// </summary>
    [Description("sha384")]
    SHA384 = 2,

    /// <summary>
    /// SHA-512 algorithm.
    /// </summary>
    [Description("sha512")]
    SHA512 = 3,
}

#nullable restore
