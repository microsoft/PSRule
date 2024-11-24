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

    private const string ENVIRONMENT_PREFIX = "PSRULE_FORMAT_";
    private const string ENVIRONMENT_TYPE_SUFFIX = "_TYPE";

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
            if (!kv.Key.EndsWith(DICTIONARY_TYPE_SUFFIX, StringComparison.OrdinalIgnoreCase) ||
                !dictionary.TryGetStringArray(kv.Key, out var type))
                return default;

            var remaining = kv.Key.Length - DICTIONARY_PREFIX.Length - DICTIONARY_TYPE_SUFFIX.Length;
            var formatName = kv.Key.Substring(DICTIONARY_PREFIX.Length, remaining).ToLowerInvariant();
            var formatType = new FormatType
            {
                Type = type
            };

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
            if (!kv.Key.EndsWith(ENVIRONMENT_TYPE_SUFFIX, StringComparison.OrdinalIgnoreCase) ||
                !Environment.TryStringArray(kv.Key, out var type))
                return default;

            var remaining = kv.Key.Length - ENVIRONMENT_PREFIX.Length - ENVIRONMENT_TYPE_SUFFIX.Length;
            var formatName = kv.Key.Substring(ENVIRONMENT_PREFIX.Length, remaining).ToLowerInvariant();
            var formatType = new FormatType
            {
                Type = type
            };

            return new KeyValuePair<string, FormatType>
            (
                formatName,
                formatType
            );
        });
    }
}
