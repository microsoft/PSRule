// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;

namespace PSRule.Configuration
{
    /// <summary>
    /// Options for repository properties that are used by PSRule.
    /// </summary>
    public sealed class RepositoryOption : IEquatable<RepositoryOption>
    {
        internal static readonly RepositoryOption Default = new()
        {

        };

        /// <summary>
        /// Create an empty repository option.
        /// </summary>
        public RepositoryOption()
        {
            BaseRef = null;
            Url = null;
        }

        /// <summary>
        /// Create a repository option by copying an existing instance.
        /// </summary>
        /// <param name="option">The option instance to copy.</param>
        public RepositoryOption(RepositoryOption option)
        {
            if (option == null)
                return;

            BaseRef = option.BaseRef;
            Url = option.Url;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is RepositoryOption option &&
                Equals(option);
        }

        /// <inheritdoc/>
        public bool Equals(RepositoryOption other)
        {
            return other != null &&
                BaseRef == other.BaseRef &&
                Url == other.Url;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                var hash = 17;
                hash = hash * 23 + (BaseRef != null ? BaseRef.GetHashCode() : 0);
                hash = hash * 23 + (Url != null ? Url.GetHashCode() : 0);
                return hash;
            }
        }

        /// <summary>
        /// Merge two option instances by repacing any unset properties from <paramref name="o1"/> with <paramref name="o2"/> values.
        /// Values from <paramref name="o1"/> that are set are not overridden.
        /// </summary>
        internal static RepositoryOption Combine(RepositoryOption o1, RepositoryOption o2)
        {
            var result = new RepositoryOption(o1)
            {
                BaseRef = o1?.BaseRef ?? o2?.BaseRef,
                Url = o1?.Url ?? o2?.Url,
            };
            return result;
        }

        /// <summary>
        /// Sets the repository base ref used for comparisons of changed files.
        /// </summary>
        [DefaultValue(null)]
        public string BaseRef { get; set; }

        /// <summary>
        /// Configures the repository URL to report in output.
        /// </summary>
        [DefaultValue(null)]
        public string Url { get; set; }

        /// <summary>
        /// Load options from environment variables into repository option.
        /// Options that appear in both will replaced by environment variable values.
        /// </summary>
        internal void Load()
        {
            if (Environment.TryString("PSRULE_REPOSITORY_BASEREF", out var baseRef))
                BaseRef = baseRef;

            if (Environment.TryString("PSRULE_REPOSITORY_URL", out var url))
                Url = url;
        }

        /// <summary>
        /// Load options from a key/ value dictionary into the repository options.
        /// Options that appear in both will replaced by dictionary values.
        /// </summary>
        internal void Load(Dictionary<string, object> index)
        {
            if (index.TryPopString("Repository.BaseRef", out var baseRef))
                BaseRef = baseRef;

            if (index.TryPopString("Repository.Url", out var url))
                Url = url;
        }
    }
}
