// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Cryptography;

namespace PSRule;

internal static class HashAlgorithmExtensions
{
    public static string GetDigest(this HashAlgorithm algorithm, byte[] buffer)
    {
        var hash = algorithm.ComputeHash(buffer);
        return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
    }

    public static string GetFileDigest(this HashAlgorithm algorithm, string path)
    {
        return algorithm.GetDigest(File.ReadAllBytes(path));
    }

    public static HashAlgorithm GetHashAlgorithm(this Options.HashAlgorithm algorithm)
    {
        if (algorithm == Options.HashAlgorithm.SHA256)
            return SHA256.Create();

        return algorithm == Options.HashAlgorithm.SHA384 ? SHA384.Create() : SHA512.Create();
    }

    public static string GetHashAlgorithmName(this Options.HashAlgorithm algorithm)
    {
        return algorithm == Options.HashAlgorithm.SHA256 ? "sha-256" : algorithm == Options.HashAlgorithm.SHA384 ? "sha-384" : "sha-512";
    }
}
