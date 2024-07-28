// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Newtonsoft.Json;
using PSRule.Definitions;
using YamlDotNet.Serialization;

namespace PSRule.Pipeline;

/// <summary>
/// A source file containing resources that will be loaded and interpreted by PSRule.
/// </summary>
[DebuggerDisplay("{Type}: {Path}")]
public sealed class SourceFile : ISourceFile
{
    private bool? _Exists;

    internal Source Source;

    /// <summary>
    /// Create an instance of a PSRule source.
    /// </summary>
    /// <param name="path">The file path to the source.</param>
    /// <param name="module">The name of the module if the source was loaded from a module.</param>
    /// <param name="type">The type of source file.</param>
    /// <param name="helpPath">The base path to use for loading help content.</param>
    public SourceFile(string path, string module, SourceType type, string helpPath)
    {
        Path = path;
        Module = module;
        Type = type;
        HelpPath = helpPath;
    }

    /// <summary>
    /// The file path to the source.
    /// </summary>
    [JsonProperty(PropertyName = "path")]
    public string Path { get; }

    /// <summary>
    /// The name of the module if the source was loaded from a module.
    /// </summary>
    [JsonProperty(PropertyName = "moduleName")]
    public string Module { get; }

    /// <summary>
    /// The type of source file.
    /// </summary>
    [YamlIgnore]
    [JsonIgnore]
    public SourceType Type { get; }

    /// <summary>
    /// The base path to use for loading help content.
    /// </summary>
    [YamlIgnore]
    [JsonIgnore]
    public string HelpPath { get; }

    /// <inheritdoc/>
    public bool Exists()
    {
        if (!_Exists.HasValue)
            _Exists = File.Exists(Path);

        return _Exists.Value;
    }

    /// <inheritdoc/>
    public bool IsDependency()
    {
        return Source.Dependency;
    }
}
