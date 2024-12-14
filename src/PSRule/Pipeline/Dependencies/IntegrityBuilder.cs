// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace PSRule.Pipeline.Dependencies;

#nullable enable

/// <summary>
/// Calculates the integrity of dependency.
/// </summary>
/// <remarks>
/// The integrity is calculated by deterministically getting the hash of each file.
/// </remarks>
public static class IntegrityBuilder
{
    private sealed class FileIntegrity(string path, string hash)
    {
        [JsonProperty("path", Order = 0)]
        public string Path { get; set; } = path;

        [JsonProperty("hash", Order = 1)]
        public string Hash { get; set; } = hash;
    }

    /// <summary>
    /// Build an integrity hash for a dependency.
    /// </summary>
    /// <param name="alg">The algorithm to use.</param>
    /// <param name="path">The directory path to the dependency.</param>
    public static LockEntryIntegrity Build(IntegrityAlgorithm alg, string path)
    {
        if (!Directory.Exists(path))
            throw new InvalidOperationException($"The path '{path}' does not exist.");

        var ignoredFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "PSGetModuleInfo.xml",
        };

        var files = GetFiles(alg, path, ignoredFiles);
        var content = JsonConvert.SerializeObject(files, new JsonSerializerSettings
        {
            Formatting = Formatting.None,
        });

        return new LockEntryIntegrity
        {
            Algorithm = IntegrityAlgorithm.SHA512,
            Hash = CalculateHashFromContent(alg, content)
        };
    }

    private static string CalculateHashFromPath(IntegrityAlgorithm alg, string path)
    {
        using var stream = File.OpenRead(path);
        using var hashingAlgorithm = GetHashAlgorithm(alg);
        var hash = hashingAlgorithm.ComputeHash(stream);

        return Convert.ToBase64String(hash);
    }

    private static string CalculateHashFromContent(IntegrityAlgorithm alg, string content)
    {
        using var hashingAlgorithm = GetHashAlgorithm(alg);
        var hash = hashingAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(content));

        return Convert.ToBase64String(hash);
    }

    private static FileIntegrity[] GetFiles(IntegrityAlgorithm alg, string path, HashSet<string> ignoredFiles)
    {
        var files = new List<FileIntegrity>();

        foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
        {
            var relativePath = ExpressionHelpers.NormalizePath(path, file, caseSensitive: true);
            if (ignoredFiles.Contains(relativePath))
                continue;

            files.Add(new FileIntegrity(
                path: relativePath,
                hash: CalculateHashFromPath(alg, file)
            ));
        }

        // Sort files by path to ensure a deterministic hash.
        files.Sort((x, y) => string.Compare(x.Path, y.Path, StringComparison.Ordinal));
        return [.. files];
    }

    private static HashAlgorithm GetHashAlgorithm(IntegrityAlgorithm alg)
    {
        return alg switch
        {
            IntegrityAlgorithm.SHA512 => SHA512.Create(),
            _ => throw new InvalidOperationException($"The integrity algorithm '{alg}' is not supported.")
        };
    }
}

#nullable restore
