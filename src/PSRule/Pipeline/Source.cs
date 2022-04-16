// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using Newtonsoft.Json;
using PSRule.Runtime;
using YamlDotNet.Serialization;

namespace PSRule.Pipeline
{
    public enum SourceType
    {
        Script = 1,

        Yaml = 2,

        Json = 3
    }

    /// <summary>
    /// A source file for rule definitions.
    /// </summary>
    [DebuggerDisplay("{Type}: {Path}")]
    public sealed class SourceFile
    {
        private bool? _Exists;

        internal Source Source;

        [JsonProperty(PropertyName = "path")]
        public string Path { get; }

        [JsonProperty(PropertyName = "moduleName")]
        public string Module { get; }

        [JsonIgnore]
        [Obsolete("Use Module property instead.")]
        public string ModuleName => Module;

        [YamlIgnore]
        [JsonIgnore]
        public SourceType Type { get; }

        [YamlIgnore]
        [JsonIgnore]
        public string HelpPath { get; }

        public SourceFile(string path, string module, SourceType type, string helpPath)
        {
            Path = path;
            Module = module;
            Type = type;
            HelpPath = helpPath;
        }

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
            public readonly string Baseline;
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

        public string Path { get; }

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
