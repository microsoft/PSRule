// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PSRule.Configuration
{
    /// <summary>
    /// Options that repository properties that are used by PSRule.
    /// </summary>
    public sealed class RepositoryOption : IEquatable<RepositoryOption>
    {
        internal static readonly RepositoryOption Default = new RepositoryOption
        {

        };

        /// <summary>
        /// Create an empty repository option.
        /// </summary>
        public RepositoryOption()
        {
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
                Url == other.Url;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                var hash = 17;
                hash = hash * 23 + (Url != null ? Url.GetHashCode() : 0);
                return hash;
            }
        }

        internal static RepositoryOption Combine(RepositoryOption o1, RepositoryOption o2)
        {
            var result = new RepositoryOption(o1)
            {
                Url = o1.Url ?? o2.Url,
            };
            return result;
        }

        /// <summary>
        /// Configures the repository URL to report in output.
        /// </summary>
        [DefaultValue(null)]
        public string Url { get; set; }

        internal void Load(EnvironmentHelper env)
        {
            if (env.TryString("PSRULE_REPOSITORY_URL", out var url))
                Url = url;
        }

        internal void Load(Dictionary<string, object> index)
        {
            if (index.TryPopString("Repository.Url", out var url))
                Url = url;
        }
    }
}
