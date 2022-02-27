// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PSRule.Configuration
{
    /// <summary>
    /// Options that configure the execution sandbox.
    /// </summary>
    public sealed class RepositoryOption : IEquatable<RepositoryOption>
    {
        internal static readonly RepositoryOption Default = new RepositoryOption
        {

        };

        public RepositoryOption()
        {
            Url = null;
        }

        public RepositoryOption(RepositoryOption option)
        {
            if (option == null)
                return;

            Url = option.Url;
        }

        public override bool Equals(object obj)
        {
            return obj is RepositoryOption option && Equals(option);
        }

        public bool Equals(RepositoryOption other)
        {
            return other != null &&
                Url == other.Url;
        }

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
        /// Determines if a warning is raised when an alias to a resource is used.
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
