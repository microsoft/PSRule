// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Management.Automation;
using PSRule.Definitions;
using PSRule.Runtime;

namespace PSRule.Pipeline;

/// <summary>
/// A PSRule source containing one or more source files.
/// </summary>
public sealed class Source
{
    internal bool Dependency;

    internal readonly ModuleInfo Module;

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

    internal sealed class ModuleInfo
    {
        private const string FIELD_PRERELEASE = "Prerelease";
        private const string FIELD_PSDATA = "PSData";
        private const string PRERELEASE_SEPARATOR = "-";

        public readonly string Path;
        public readonly string Name;
        public readonly string Version;
        public readonly string ProjectUri;
        public readonly string Guid;
        public readonly string CompanyName;

        public ModuleInfo(PSModuleInfo info)
        {
            Path = info.ModuleBase;
            Name = info.Name;
            Version = info.Version?.ToString();
            ProjectUri = info.ProjectUri?.ToString();
            Guid = info.Guid.ToString();
            CompanyName = info.CompanyName;
            if (TryPrivateData(info, FIELD_PSDATA, out var psData) && psData.ContainsKey(FIELD_PRERELEASE))
                Version = string.Concat(Version, PRERELEASE_SEPARATOR, psData[FIELD_PRERELEASE].ToString());
        }

        public ModuleInfo(string path, string name, string version, string projectUri, string guid, string companyName, string prerelease)
        {
            Path = path;
            Name = name;
            Version = version;
            ProjectUri = projectUri;
            Guid = guid;
            CompanyName = companyName;
            if (!string.IsNullOrEmpty(prerelease))
                Version = string.Concat(version, PRERELEASE_SEPARATOR, prerelease);
        }

        private static bool TryPrivateData(PSModuleInfo info, string propertyName, out Hashtable value)
        {
            value = null;
            if (info.PrivateData is Hashtable privateData && privateData.ContainsKey(propertyName) && privateData[propertyName] is Hashtable data)
            {
                value = data;
                return true;
            }
            return false;
        }
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
