// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;

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
        /// Returns an array of Key/Values.
        /// </summary>
        public ModuleConstraint[] ToArray()
        {
            var result = new List<ModuleConstraint>();
            foreach (var kv in this)
            {
                if (SemanticVersion.TryParseConstraint(kv.Value, out var constraint))
                    result.Add(new ModuleConstraint(kv.Key, constraint));
            }
            return result.ToArray();
        }

        /// <summary>
        /// Return the module constaints as a dictionary indexed by module name.
        /// </summary>
        public IDictionary<string, ModuleConstraint> ToDictionary()
        {
            var result = new Dictionary<string, ModuleConstraint>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in this)
            {
                if (SemanticVersion.TryParseConstraint(kv.Value, out var constraint))
                    result.Add(kv.Key, new ModuleConstraint(kv.Key, constraint));
            }
            return result;
        }

        /// <summary>
        /// Merge two option instances by replacing any unset properties from <paramref name="o1"/> with <paramref name="o2"/> values.
        /// Values from <paramref name="o1"/> that are set are not overridden.
        /// </summary>
        internal static RequiresOption Combine(RequiresOption o1, RequiresOption o2)
        {
            var result = new RequiresOption(o1);
            result.AddUnique(o2);
            return result;
        }

        /// <summary>
        /// Load Requires option from environment variables.
        /// </summary>
        internal void Load()
        {
            Load(ENVIRONMENT_PREFIX, ConvertUnderscore);
        }

        /// <summary>
        /// Load Requires option from a dictionary.
        /// </summary>
        internal void Load(IDictionary<string, object> dictionary)
        {
            Load(DICTIONARY_PREFIX, dictionary);
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
