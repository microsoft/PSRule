// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;

namespace PSRule.Configuration
{
    /// <summary>
    /// Options that affect source locations imported for execution.
    /// </summary>
    public sealed class IncludeOption : IEquatable<IncludeOption>
    {
        private const string[] DEFAULT_MODULE = null;
        private const string[] DEFAULT_PATH = null;

        internal static readonly IncludeOption Default = new()
        {
            Path = DEFAULT_PATH,
            Module = DEFAULT_MODULE,
        };

        /// <summary>
        /// Create an empty include option.
        /// </summary>
        public IncludeOption()
        {
            Path = null;
            Module = null;
        }

        /// <summary>
        /// Create an include option by copying an existing instance.
        /// </summary>
        /// <param name="option">The option instance to copy.</param>
        public IncludeOption(IncludeOption option)
        {
            if (option == null)
                return;

            Path = option.Path;
            Module = option.Module;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is IncludeOption option && Equals(option);
        }

        /// <inheritdoc/>
        public bool Equals(IncludeOption other)
        {
            return other != null &&
                Path == other.Path &&
                Module == other.Module;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                var hash = 17;
                hash = hash * 23 + (Path != null ? Path.GetHashCode() : 0);
                hash = hash * 23 + (Module != null ? Module.GetHashCode() : 0);
                return hash;
            }
        }

        internal static IncludeOption Combine(IncludeOption o1, IncludeOption o2)
        {
            var result = new IncludeOption(o1)
            {
                Path = o1?.Path ?? o2?.Path,
                Module = o1?.Module ?? o2?.Module
            };
            return result;
        }

        /// <summary>
        /// Include additional module sources.
        /// </summary>
        [DefaultValue(null)]
        public string[] Path { get; set; }

        /// <summary>
        /// Include additional standalone sources.
        /// </summary>
        [DefaultValue(null)]
        public string[] Module { get; set; }

        internal void Load()
        {
            if (Environment.TryStringArray("PSRULE_INCLUDE_PATH", out var path))
                Path = path;

            if (Environment.TryStringArray("PSRULE_INCLUDE_MODULE", out var module))
                Module = module;
        }

        internal void Load(Dictionary<string, object> index)
        {
            if (index.TryPopStringArray("Include.Path", out var path))
                Path = path;

            if (index.TryPopStringArray("Include.Module", out var module))
                Module = module;
        }
    }

}
