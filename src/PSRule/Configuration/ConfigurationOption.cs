// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace PSRule.Configuration;

/// <summary>
/// A set of configuration values that can be used within rule definitions.
/// </summary>
public sealed class ConfigurationOption : KeyMapDictionary<object>
{
    private const string ENVIRONMENT_PREFIX = "PSRULE_CONFIGURATION_";
    private const string DICTIONARY_PREFIX = "Configuration.";

    /// <summary>
    /// Creates an empty configuration option.
    /// </summary>
    public ConfigurationOption()
        : base() { }

    /// <summary>
    /// Creates a configuration option by copying an existing instance.
    /// </summary>
    /// <param name="option">The option instance to copy.</param>
    public ConfigurationOption(ConfigurationOption option)
        : base(option) { }

    /// <summary>
    /// Creates a configuration option from a hashtable.
    /// </summary>
    private ConfigurationOption(Hashtable hashtable)
        : base(hashtable) { }

    /// <summary>
    /// Convert a hashtable (commonly used in PowerShell) to a configuration option.
    /// </summary>
    /// <param name="hashtable">The hashtable to convert.</param>
    public static implicit operator ConfigurationOption(Hashtable hashtable)
    {
        return new ConfigurationOption(hashtable);
    }

    /// <summary>
    /// Merge two option instances by replacing any unset properties from <paramref name="o1"/> with <paramref name="o2"/> values.
    /// Values from <paramref name="o1"/> that are set are not overridden.
    /// </summary>
    internal static ConfigurationOption Combine(ConfigurationOption o1, ConfigurationOption o2)
    {
        var result = new ConfigurationOption(o1);
        result.AddUnique(o2);
        return result;
    }

    /// <summary>
    /// Load values from environment variables into the configuration option.
    /// Keys that appear in both will replaced by environment variable values.
    /// </summary>
    internal void Load()
    {
        Load(ENVIRONMENT_PREFIX);
    }

    /// <summary>
    /// Load values from a key/ value dictionary into the configuration option.
    /// Keys that appear in both will replaced by dictionary values.
    /// </summary>
    internal void Load(IDictionary<string, object> dictionary)
    {
        Load(DICTIONARY_PREFIX, dictionary);
    }
}
