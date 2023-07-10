// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;

namespace PSRule.Configuration
{
    /// <summary>
    /// Options that affect how input types are processed.
    /// </summary>
    public sealed class InputOption : IEquatable<InputOption>
    {
        private const InputFormat DEFAULT_FORMAT = InputFormat.Detect;
        private const bool DEFAULT_IGNOREGITPATH = true;
        private const bool DEFAULT_IGNOREOBJECTSOURCE = false;
        private const bool DEFAULT_IGNOREREPOSITORYCOMMON = true;
        private const bool DEFAULT_IGNOREUNCHANGEDPATH = false;
        private const string DEFAULT_OBJECTPATH = null;
        private const string[] DEFAULT_PATHIGNORE = null;
        private const string[] DEFAULT_TARGETTYPE = null;

        internal static readonly InputOption Default = new()
        {
            Format = DEFAULT_FORMAT,
            IgnoreGitPath = DEFAULT_IGNOREGITPATH,
            IgnoreObjectSource = DEFAULT_IGNOREOBJECTSOURCE,
            IgnoreRepositoryCommon = DEFAULT_IGNOREREPOSITORYCOMMON,
            IgnoreUnchangedPath = DEFAULT_IGNOREUNCHANGEDPATH,
            ObjectPath = DEFAULT_OBJECTPATH,
            PathIgnore = DEFAULT_PATHIGNORE,
            TargetType = DEFAULT_TARGETTYPE,
        };

        /// <summary>
        /// Creates an empty input option.
        /// </summary>
        public InputOption()
        {
            Format = null;
            IgnoreGitPath = null;
            IgnoreObjectSource = null;
            IgnoreRepositoryCommon = null;
            IgnoreUnchangedPath = null;
            ObjectPath = null;
            PathIgnore = null;
            TargetType = null;
        }

        /// <summary>
        /// Creates a input option by copying an existing instance.
        /// </summary>
        /// <param name="option">The option instance to copy.</param>
        public InputOption(InputOption option)
        {
            if (option == null)
                return;

            Format = option.Format;
            IgnoreGitPath = option.IgnoreGitPath;
            IgnoreObjectSource = option.IgnoreObjectSource;
            IgnoreRepositoryCommon = option.IgnoreRepositoryCommon;
            IgnoreUnchangedPath = option.IgnoreUnchangedPath;
            ObjectPath = option.ObjectPath;
            PathIgnore = option.PathIgnore;
            TargetType = option.TargetType;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is InputOption option && Equals(option);
        }

        /// <inheritdoc/>
        public bool Equals(InputOption other)
        {
            return other != null &&
                Format == other.Format &&
                IgnoreGitPath == other.IgnoreGitPath &&
                IgnoreObjectSource == other.IgnoreObjectSource &&
                IgnoreRepositoryCommon == other.IgnoreRepositoryCommon &&
                IgnoreUnchangedPath == other.IgnoreUnchangedPath &&
                ObjectPath == other.ObjectPath &&
                PathIgnore == other.PathIgnore &&
                TargetType == other.TargetType;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                var hash = 17;
                hash = hash * 23 + (Format.HasValue ? Format.Value.GetHashCode() : 0);
                hash = hash * 23 + (IgnoreGitPath.HasValue ? IgnoreGitPath.Value.GetHashCode() : 0);
                hash = hash * 23 + (IgnoreObjectSource.HasValue ? IgnoreObjectSource.Value.GetHashCode() : 0);
                hash = hash * 23 + (IgnoreRepositoryCommon.HasValue ? IgnoreRepositoryCommon.Value.GetHashCode() : 0);
                hash = hash * 23 + (IgnoreUnchangedPath.HasValue ? IgnoreUnchangedPath.Value.GetHashCode() : 0);
                hash = hash * 23 + (ObjectPath != null ? ObjectPath.GetHashCode() : 0);
                hash = hash * 23 + (PathIgnore != null ? PathIgnore.GetHashCode() : 0);
                hash = hash * 23 + (TargetType != null ? TargetType.GetHashCode() : 0);
                return hash;
            }
        }

        /// <summary>
        /// Merge two option instances by repacing any unset properties from <paramref name="o1"/> with <paramref name="o2"/> values.
        /// Values from <paramref name="o1"/> that are set are not overridden.
        /// </summary>
        internal static InputOption Combine(InputOption o1, InputOption o2)
        {
            var result = new InputOption(o1)
            {
                Format = o1.Format ?? o2.Format,
                IgnoreGitPath = o1.IgnoreGitPath ?? o2.IgnoreGitPath,
                IgnoreObjectSource = o1.IgnoreObjectSource ?? o2.IgnoreObjectSource,
                IgnoreRepositoryCommon = o1.IgnoreRepositoryCommon ?? o2.IgnoreRepositoryCommon,
                IgnoreUnchangedPath = o1.IgnoreUnchangedPath ?? o2.IgnoreUnchangedPath,
                ObjectPath = o1.ObjectPath ?? o2.ObjectPath,
                PathIgnore = o1.PathIgnore ?? o2.PathIgnore,
                TargetType = o1.TargetType ?? o2.TargetType
            };
            return result;
        }

        /// <summary>
        /// The input string format.
        /// </summary>
        [DefaultValue(null)]
        public InputFormat? Format { get; set; }

        /// <summary>
        /// Determine if files within the .git path are ignored.
        /// </summary>
        [DefaultValue(null)]
        public bool? IgnoreGitPath { get; set; }

        /// <summary>
        /// Determines if objects are ignored based on their file source path.
        /// </summary>
        [DefaultValue(null)]
        public bool? IgnoreObjectSource { get; set; }

        /// <summary>
        /// Determine if common repository files are ignored.
        /// </summary>
        [DefaultValue(null)]
        public bool? IgnoreRepositoryCommon { get; set; }

        /// <summary>
        /// Determine if unchanged files are ignored.
        /// </summary>
        [DefaultValue(null)]
        public bool? IgnoreUnchangedPath { get; set; }

        /// <summary>
        /// The object path to a property to use instead of the pipeline object.
        /// </summary>
        [DefaultValue(null)]
        public string ObjectPath { get; set; }

        /// <summary>
        /// Ignores input files that match the path spec.
        /// </summary>
        [DefaultValue(null)]
        public string[] PathIgnore { get; set; }

        /// <summary>
        /// Only process objects that match one of the included types.
        /// </summary>
        [DefaultValue(null)]
        public string[] TargetType { get; set; }

        internal void Load()
        {
            if (Environment.TryEnum("PSRULE_INPUT_FORMAT", out InputFormat format))
                Format = format;

            if (Environment.TryBool("PSRULE_INPUT_IGNOREGITPATH", out var ignoreGitPath))
                IgnoreGitPath = ignoreGitPath;

            if (Environment.TryBool("PSRULE_INPUT_IGNOREOBJECTSOURCE", out var ignoreObjectSource))
                IgnoreObjectSource = ignoreObjectSource;

            if (Environment.TryBool("PSRULE_INPUT_IGNOREREPOSITORYCOMMON", out var ignoreRepositoryCommon))
                IgnoreRepositoryCommon = ignoreRepositoryCommon;

            if (Environment.TryBool("PSRULE_INPUT_IGNOREUNCHANGEDPATH", out var ignoreUnchangedPath))
                IgnoreUnchangedPath = ignoreUnchangedPath;

            if (Environment.TryString("PSRULE_INPUT_OBJECTPATH", out var objectPath))
                ObjectPath = objectPath;

            if (Environment.TryStringArray("PSRULE_INPUT_PATHIGNORE", out var pathIgnore))
                PathIgnore = pathIgnore;

            if (Environment.TryStringArray("PSRULE_INPUT_TARGETTYPE", out var targetType))
                TargetType = targetType;
        }

        internal void Load(Dictionary<string, object> index)
        {
            if (index.TryPopEnum("Input.Format", out InputFormat format))
                Format = format;

            if (index.TryPopBool("Input.IgnoreGitPath", out var ignoreGitPath))
                IgnoreGitPath = ignoreGitPath;

            if (index.TryPopBool("Input.IgnoreObjectSource", out var ignoreObjectSource))
                IgnoreObjectSource = ignoreObjectSource;

            if (index.TryPopBool("Input.IgnoreRepositoryCommon", out var ignoreRepositoryCommon))
                IgnoreRepositoryCommon = ignoreRepositoryCommon;

            if (index.TryPopBool("Input.IgnoreUnchangedPath", out var ignoreUnchangedPath))
                IgnoreUnchangedPath = ignoreUnchangedPath;

            if (index.TryPopString("Input.ObjectPath", out var objectPath))
                ObjectPath = objectPath;

            if (index.TryPopStringArray("Input.PathIgnore", out var pathIgnore))
                PathIgnore = pathIgnore;

            if (index.TryPopStringArray("Input.TargetType", out var targetType))
                TargetType = targetType;
        }
    }
}
