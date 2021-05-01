// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace PSRule.Configuration
{
    public sealed class RequiresOption : KeyMapDictionary<string>
    {
        private const string ENVIRONMENT_PREFIX = "PSRULE_REQUIRES_";
        private const string DICTIONARY_PREFIX = "Requires.";
        private const char UNDERSCORE = '_';
        private const char DOT = '.';

        public RequiresOption()
            : base() { }

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
