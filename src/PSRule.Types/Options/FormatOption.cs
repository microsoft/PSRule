// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule.Options;

/// <summary>
/// Options that configure format types.
/// </summary>
/// <remarks>
/// See <see href="https://aka.ms/ps-rule/options"/>.
/// </remarks>
public sealed class FormatOption : StringMap<FormatType>, IFormatOption
{
    private const string DICTIONARY_PREFIX = "Format.";
    private const string DICTIONARY_TYPE_SUFFIX = ".Type";
    private const string DICTIONARY_ENABLED_SUFFIX = ".Enabled";
    private const string DICTIONARY_REPLACE_SUFFIX = ".Replace";

    private const string ENVIRONMENT_PREFIX = "PSRULE_FORMAT_";
    private const string ENVIRONMENT_TYPE_SUFFIX = "_TYPE";
    private const string ENVIRONMENT_ENABLED_SUFFIX = "_ENABLED";
    private const string ENVIRONMENT_REPLACE_SUFFIX = "_REPLACE";

    internal static readonly FormatOption Default = [];

    /// <summary>
    /// Creates an empty format option.
    /// </summary>
    public FormatOption()
        : base() { }

    /// <summary>
    /// Creates a format option by copying an existing instance.
    /// </summary>
    public FormatOption(FormatOption option)
        : base(option) { }

    /// <summary>
    /// Merge two option instances by replacing any unset properties from <paramref name="o1"/> with <paramref name="o2"/> values.
    /// Values from <paramref name="o1"/> that are set are not overridden.
    /// </summary>
    internal static FormatOption Combine(FormatOption o1, FormatOption o2)
    {
        var option = new FormatOption();
        option.Combine(o1);
        option.Combine(o2);
        return option;
    }

    /// <inheritdoc/>
    public void Import(IDictionary<string, object> dictionary)
    {
        // Format.<FORMAT>.Type
        ImportFromDictionary(DICTIONARY_PREFIX, dictionary, kv =>
        {
            FormatType? formatType = null;

            if (TryKeySuffix(kv.Key, DICTIONARY_PREFIX, DICTIONARY_TYPE_SUFFIX, out var formatName) &&
                dictionary.TryGetStringArray(kv.Key, out var type) &&
                type != null)
            {
                formatType = new FormatType
                {
                    Type = type
                };
            }

            else if (TryKeySuffix(kv.Key, DICTIONARY_PREFIX, DICTIONARY_ENABLED_SUFFIX, out formatName) &&
                dictionary.TryGetBool(kv.Key, out var enabled) &&
                enabled != null)
            {
                formatType = new FormatType
                {
                    Enabled = enabled
                };
            }

            else if (TryKeySuffix(kv.Key, DICTIONARY_PREFIX, DICTIONARY_REPLACE_SUFFIX, out formatName) &&
                dictionary.TryGetDictionary<string>(kv.Key, out var replace) &&
                replace != null)
            {
                formatType = new FormatType
                {
                    Replace = [.. replace]
                };
            }

            // Ignore invalid format configurations.
            if (formatName == null || formatType == null)
                return default;

            // Merge with existing format type.
            if (TryGetValue(formatName, out var existing))
            {
                formatType = FormatType.Combine(formatType, existing);
            }

            return new KeyValuePair<string, FormatType>
            (
                formatName,
                formatType
            );
        });
    }

    /// <inheritdoc/>
    public void Load()
    {
        // PSRULE_FORMAT_<FORMAT>_TYPE='<value>'
        ImportFromEnvironmentVariables(ENVIRONMENT_PREFIX, kv =>
        {
            FormatType? formatType = null;

            if (TryKeySuffix(kv.Key, ENVIRONMENT_PREFIX, ENVIRONMENT_TYPE_SUFFIX, out var formatName) &&
                Environment.TryStringArray(kv.Key, out var type) &&
                type != null)
            {
                formatType = new FormatType
                {
                    Type = type
                };
            }

            else if (TryKeySuffix(kv.Key, ENVIRONMENT_PREFIX, ENVIRONMENT_ENABLED_SUFFIX, out formatName) &&
                Environment.TryBool(kv.Key, out var enabled) &&
                enabled != null)
            {
                formatType = new FormatType
                {
                    Enabled = enabled
                };
            }

            else if (TryKeySuffix(kv.Key, ENVIRONMENT_PREFIX, ENVIRONMENT_REPLACE_SUFFIX, out formatName) &&
                Environment.TryDictionary<string>(kv.Key, out var replace) &&
                replace != null)
            {
                formatType = new FormatType
                {
                    Replace = [.. replace]
                };
            }

            // Ignore invalid format configurations.
            if (formatName == null || formatType == null)
                return default;

            // Merge with existing format type.
            if (TryGetValue(formatName, out var existing))
            {
                formatType = FormatType.Combine(formatType, existing);
            }

            return new KeyValuePair<string, FormatType>
            (
                formatName,
                formatType
            );
        });
    }

    private static bool TryKeySuffix(string key, string prefix, string suffix, out string? formatName)
    {
        formatName = null;
        if (prefix == null || prefix.Length == 0)
            return false;

        if (key.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            formatName = key.Substring(prefix.Length, key.Length - prefix.Length - suffix.Length).ToLowerInvariant();
            return true;
        }
        return false;
    }
}
