// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace PSRule.Configuration
{
    /// <summary>
    /// Specifies module version constraints for running PSRule.
    /// When set, PSRule will error if a module version is used that does not satisfy the requirements.
    /// <seealso href="https://microsoft.github.io/PSRule/latest/concepts/PSRule/en-US/about_PSRule_Options/#requires"/>
    /// </summary>
    public sealed class RequiresOption : KeyMapDictionary<string>
    {
        private const string ENVIRONMENT_PREFIX = "PSRULE_REQUIRES_";
        private const string DICTIONARY_PREFIX = "Requires.";
        private const char UNDERSCORE = '_';
        private const char DOT = '.';

        /// <summary>
        /// Creates an empty requires option.
        /// </summary>
        public RequiresOption()
            : base() { }

        /// <summary>
        /// Creates a requires option by copying an existing instance.
        /// </summary>
        /// <param name="option">The option instance to copy.</param>
        internal RequiresOption(RequiresOption option)
            : base(option) { }

        /// <summary>
        /// Load Requires option from environment variables.
        /// </summary>
        internal void Load(EnvironmentHelper env)
        {
            base.Load(ENVIRONMENT_PREFIX, env, ConvertUnderscore);
        }

        /// <summary>
        /// Load Requires option from a dictionary.
        /// </summary>
        internal void Load(IDictionary<string, object> dictionary)
        {
            base.Load(DICTIONARY_PREFIX, dictionary);
        }

        /// <summary>
        /// Convert module names with underscores to dots.
        /// </summary>
        private string ConvertUnderscore(string key)
        {
            return key.Replace(UNDERSCORE, DOT);
        }
    }
}
