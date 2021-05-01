// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PSRule.Configuration
{
    /// <summary>
    /// Options that affect how input types are processed.
    /// </summary>
    public sealed class InputOption : IEquatable<InputOption>
    {
        private const InputFormat DEFAULT_FORMAT = PSRule.Configuration.InputFormat.Detect;
        private const bool DEFAULT_IGNOREGITPATH = true;
        private const string DEFAULT_OBJECTPATH = null;
        private const string[] DEFAULT_PATHIGNORE = null;
        private const string[] DEFAULT_TARGETTYPE = null;

        internal static readonly InputOption Default = new InputOption
        {
            Format = DEFAULT_FORMAT,
            IgnoreGitPath = DEFAULT_IGNOREGITPATH,
            ObjectPath = DEFAULT_OBJECTPATH,
            PathIgnore = DEFAULT_PATHIGNORE,
            TargetType = DEFAULT_TARGETTYPE,
        };

        public InputOption()
        {
            Format = null;
            IgnoreGitPath = null;
            ObjectPath = null;
            PathIgnore = null;
            TargetType = null;
        }

        public InputOption(InputOption option)
        {
            if (option == null)
                return;

            Format = option.Format;
            IgnoreGitPath = option.IgnoreGitPath;
            ObjectPath = option.ObjectPath;
            PathIgnore = option.PathIgnore;
            TargetType = option.TargetType;
        }

        public override bool Equals(object obj)
        {
            return obj is InputOption option && Equals(option);
        }

        public bool Equals(InputOption other)
        {
            return other != null &&
                Format == other.Format &&
                IgnoreGitPath == other.IgnoreGitPath &&
                ObjectPath == other.ObjectPath &&
                PathIgnore == other.PathIgnore &&
                TargetType == other.TargetType;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                int hash = 17;
                hash = hash * 23 + (Format.HasValue ? Format.Value.GetHashCode() : 0);
                hash = hash * 23 + (IgnoreGitPath.HasValue ? IgnoreGitPath.Value.GetHashCode() : 0);
                hash = hash * 23 + (ObjectPath != null ? ObjectPath.GetHashCode() : 0);
                hash = hash * 23 + (PathIgnore != null ? PathIgnore.GetHashCode() : 0);
                hash = hash * 23 + (TargetType != null ? TargetType.GetHashCode() : 0);
                return hash;
            }
        }

        internal static InputOption Combine(InputOption o1, InputOption o2)
        {
            var result = new InputOption(o1)
            {
                Format = o1.Format ?? o2.Format,
                IgnoreGitPath = o1.IgnoreGitPath ?? o2.IgnoreGitPath,
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

        internal void Load(EnvironmentHelper env)
        {
            if (env.TryEnum("PSRULE_INPUT_FORMAT", out InputFormat format))
                Format = format;

            if (env.TryBool("PSRULE_INPUT_IGNOREGITPATH", out bool ignoreGitPath))
                IgnoreGitPath = ignoreGitPath;

            if (env.TryString("PSRULE_INPUT_OBJECTPATH", out string objectPath))
                ObjectPath = objectPath;

            if (env.TryStringArray("PSRULE_INPUT_PATHIGNORE", out string[] pathIgnore))
                PathIgnore = pathIgnore;

            if (env.TryStringArray("PSRULE_INPUT_TARGETTYPE", out string[] targetType))
                TargetType = targetType;
        }

        internal void Load(Dictionary<string, object> index)
        {
            if (index.TryPopEnum("Input.Format", out InputFormat format))
                Format = format;

            if (index.TryPopBool("Input.IgnoreGitPath", out bool ignoreGitPath))
                IgnoreGitPath = ignoreGitPath;

            if (index.TryPopString("Input.ObjectPath", out string objectPath))
                ObjectPath = objectPath;

            if (index.TryPopStringArray("Input.PathIgnore", out string[] pathIgnore))
                PathIgnore = pathIgnore;

            if (index.TryPopStringArray("Input.TargetType", out string[] targetType))
                TargetType = targetType;
        }
    }
}
