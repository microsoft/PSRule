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
}
