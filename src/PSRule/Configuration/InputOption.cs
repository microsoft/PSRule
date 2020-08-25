// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;

namespace PSRule.Configuration
{
    /// <summary>
    /// Options that affect how input types are processed.
    /// </summary>
    public sealed class InputOption : IEquatable<InputOption>
    {
        private const InputFormat DEFAULT_FORMAT = PSRule.Configuration.InputFormat.Detect;
        private const string DEFAULT_OBJECTPATH = null;
        private const string[] DEFAULT_PATHIGNORE = null;
        private const string[] DEFAULT_TARGETTYPE = null;

        internal static readonly InputOption Default = new InputOption
        {
            Format = DEFAULT_FORMAT,
            ObjectPath = DEFAULT_OBJECTPATH,
            PathIgnore = DEFAULT_PATHIGNORE,
            TargetType = DEFAULT_TARGETTYPE,
        };

        public InputOption()
        {
            Format = null;
            ObjectPath = null;
            PathIgnore = null;
            TargetType = null;
        }

        public InputOption(InputOption option)
        {
            if (option == null)
                return;

            Format = option.Format;
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
                hash = hash * 23 + (ObjectPath != null ? ObjectPath.GetHashCode() : 0);
                hash = hash * 23 + (PathIgnore != null ? PathIgnore.GetHashCode() : 0);
                hash = hash * 23 + (TargetType != null ? TargetType.GetHashCode() : 0);
                return hash;
            }
        }

        internal static InputOption Combine(InputOption o1, InputOption o2)
        {
            var result = new InputOption(o1);
            result.Format = o1.Format ?? o2.Format;
            result.ObjectPath = o1.ObjectPath ?? o2.ObjectPath;
            result.PathIgnore = o1.PathIgnore ?? o2.PathIgnore;
            result.TargetType = o1.TargetType ?? o2.TargetType;
            return result;
        }

        /// <summary>
        /// The input string format.
        /// </summary>
        [DefaultValue(null)]
        public InputFormat? Format { get; set; }

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
    }
}
