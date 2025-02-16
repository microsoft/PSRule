// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using PSRule.Definitions;

namespace PSRule.Pipeline;

#nullable enable

/// <summary>
/// A PSRule source containing one or more source files.
/// </summary>
public sealed class Source
{
    internal bool Dependency;

    internal readonly ModuleInfo? Module;

    internal Source(string path, SourceFile[] file)
    {
        Path = path;
        Module = null;
        File = file;
        Dependency = false;
        SetSource();
    }

    internal Source(ModuleInfo module, SourceFile[] file, bool dependency)
    {
        Path = module.Path;
        Module = module;
        File = file;
        Dependency = dependency;
        SetSource();
    }

    internal sealed class ModuleInfo(string path, string name, string version, string? projectUri, string? guid, string? companyName, string? prerelease, Assembly[] assemblies)
    {
        private const string PRERELEASE_SEPARATOR = "-";

        public readonly string Path = path;
        public readonly string Name = name;
        public readonly string Version = version;
        public readonly string FullVersion = string.IsNullOrEmpty(prerelease) ? version : string.Concat(version, PRERELEASE_SEPARATOR, prerelease);

        public readonly Assembly[] Assemblies = assemblies;

        public readonly string? ProjectUri = projectUri;
        public readonly string? Guid = guid;
        public readonly string? CompanyName = companyName;
    }

    /// <summary>
    /// The base path of the source.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// An array of source files.
    /// </summary>
    public SourceFile[] File { get; }

    internal string Scope
    {
        get
        {
            return ResourceHelper.NormalizeScope(Module?.Name);
        }
    }

    private void SetSource()
    {
        for (var i = 0; File != null && i < File.Length; i++)
            File[i].Source = this;
    }
}

#nullable restore
