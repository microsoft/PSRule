// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Diagnostics;
using System.Management.Automation;
using Newtonsoft.Json;
using PSRule.Runtime;
using YamlDotNet.Serialization;

namespace PSRule.Pipeline
{
    /// <summary>
    /// The type of source file.
    /// </summary>
    public enum SourceType
    {
        /// <summary>
        /// PowerShell script file.
        /// </summary>
        Script = 1,

        /// <summary>
        /// YAML file.
        /// </summary>
        Yaml = 2,

        /// <summary>
        /// JSON or JSON with comments file.
        /// </summary>
        Json = 3
    }

    /// <summary>
    /// A source file containing resources that will be loaded and interpreted by PSRule.
    /// </summary>
    [DebuggerDisplay("{Type}: {Path}")]
    public sealed class SourceFile
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
        /// The name of the module if the source was loaded from a module.
        /// </summary>
        [JsonIgnore]
        [Obsolete("Use Module property instead.")]
        public string ModuleName => Module;

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

        internal bool Exists()
        {
            if (!_Exists.HasValue)
                _Exists = File.Exists(Path);

            return _Exists.Value;
        }

        internal bool IsDependency()
        {
            return Source.Dependency;
        }
    }

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
                return LanguageScope.Normalize(Module?.Name);
            }
        }

        private void SetSource()
        {
            for (var i = 0; File != null && i < File.Length; i++)
                File[i].Source = this;
        }
    }
}
