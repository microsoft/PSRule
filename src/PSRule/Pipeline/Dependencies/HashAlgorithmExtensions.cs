// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Options;

namespace PSRule.Pipeline.Dependencies;

/// <summary>
/// Extension for <see cref="HashAlgorithm"/>.
/// </summary>
public static class HashAlgorithmExtensions
{
    /// <summary>
    /// Convert a <see cref="HashAlgorithm"/> to <see cref="IntegrityAlgorithm"/>.
    /// </summary>
    public static IntegrityAlgorithm ToIntegrityAlgorithm(this HashAlgorithm hashAlgorithm)
    {
        return hashAlgorithm switch
        {
            HashAlgorithm.SHA256 => IntegrityAlgorithm.SHA256,
            HashAlgorithm.SHA384 => IntegrityAlgorithm.SHA384,
            HashAlgorithm.SHA512 => IntegrityAlgorithm.SHA512,
            _ => IntegrityAlgorithm.Unknown
        };
    }
}
