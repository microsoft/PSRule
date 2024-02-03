// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
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
}

/// <summary>
/// Define the structure for the PSRule lock file.
/// By default, this file is <c>ps-rule.lock.json</c>.
/// </summary>
public sealed class LockFile
{
    private const string DEFAULT_FILE = "ps-rule.lock.json";

    /// <summary>
    /// The version of the lock file schema.
    /// </summary>
    [JsonProperty("version")]
    public int Version { get; set; }

    /// <summary>
    /// A mapping lock file entries for each module.
    /// </summary>
    [JsonProperty("modules")]
    public Dictionary<string, LockEntry> Modules { get; set; } = new Dictionary<string, LockEntry>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Read the lock file from disk.
    /// </summary>
    /// <param name="path">An alternative path to the lock file.</param>
    /// <returns>Returns an instance of the lock file or a default instance if the file does not exist.</returns>
    public static LockFile Read(string path)
    {
        path = Environment.GetRootedPath(path);
        path = Path.GetExtension(path) == ".json" ? path : Path.Combine(path, DEFAULT_FILE);
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path, Encoding.UTF8);
            return JsonConvert.DeserializeObject<LockFile>(json, new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new SemanticVersionConverter()
                },
            });
        }
        return new LockFile();
    }

    /// <summary>
    /// Write the lock file to disk.
    /// </summary>
    /// <param name="path">An alternative path to the lock file.</param>
    public void Write(string path)
    {
        Version = 1;

        path = Environment.GetRootedPath(path);
        path = Path.GetExtension(path) == "json" ? path : Path.Combine(path, DEFAULT_FILE);
        var json = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new SemanticVersionConverter()
            }
        });
        File.WriteAllText(path, json, Encoding.UTF8);
    }
}
