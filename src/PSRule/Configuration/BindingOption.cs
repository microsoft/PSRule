// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.ComponentModel;

namespace PSRule.Configuration;

/// <summary>
/// Options that configure property binding.
/// </summary>
/// <remarks>
/// See <see href="https://aka.ms/ps-rule/options"/>.
/// </remarks>
public sealed class BindingOption : IEquatable<BindingOption>, IBindingOption
{
    private const bool DEFAULT_IGNORE_CASE = true;
    private const string DEFAULT_NAME_SEPARATOR = "/";
    private const bool DEFAULT_USE_QUALIFIED_NAME = false;

    internal static readonly BindingOption Default = new()
    {
        IgnoreCase = DEFAULT_IGNORE_CASE,
        NameSeparator = DEFAULT_NAME_SEPARATOR,
        UseQualifiedName = DEFAULT_USE_QUALIFIED_NAME
    };

    /// <summary>
    /// Creates an empty binding option.
    /// </summary>
    public BindingOption()
    {
        Field = null;
        IgnoreCase = null;
        NameSeparator = null;
        TargetName = null;
        TargetType = null;
        UseQualifiedName = null;
    }

    /// <summary>
    /// Creates a binding option by copying an existing instance.
    /// </summary>
    /// <param name="option">The option instance to copy.</param>
    public BindingOption(BindingOption option)
    {
        if (option == null)
            return;

        Field = option.Field;
        IgnoreCase = option.IgnoreCase;
        NameSeparator = option.NameSeparator;
        TargetName = option.TargetName;
        TargetType = option.TargetType;
        UseQualifiedName = option.UseQualifiedName;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is BindingOption option && Equals(option);
    }

    /// <inheritdoc/>
    public bool Equals(BindingOption other)
    {
        return other != null &&
            Field == other.Field &&
            IgnoreCase == other.IgnoreCase &&
            NameSeparator == other.NameSeparator &&
            TargetName == other.TargetName &&
            TargetType == other.TargetType &&
            UseQualifiedName == other.UseQualifiedName;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked // Overflow is fine
        {
            var hash = 17;
            hash = hash * 23 + (Field != null ? Field.GetHashCode() : 0);
            hash = hash * 23 + (IgnoreCase.HasValue ? IgnoreCase.Value.GetHashCode() : 0);
            hash = hash * 23 + (NameSeparator != null ? NameSeparator.GetHashCode() : 0);
            hash = hash * 23 + (TargetName != null ? TargetName.GetHashCode() : 0);
            hash = hash * 23 + (TargetType != null ? TargetType.GetHashCode() : 0);
            hash = hash * 23 + (UseQualifiedName.HasValue ? UseQualifiedName.Value.GetHashCode() : 0);
            return hash;
        }
    }

    /// <summary>
    /// Merge two option instances by replacing any unset properties from <paramref name="o1"/> with <paramref name="o2"/> values.
    /// Values from <paramref name="o1"/> that are set are not overridden.
    /// </summary>
    internal static BindingOption Combine(BindingOption o1, BindingOption o2)
    {
        var result = new BindingOption(o1)
        {
            Field = FieldMap.Combine(o1?.Field, o2?.Field),
            IgnoreCase = o1?.IgnoreCase ?? o2?.IgnoreCase,
            NameSeparator = o1?.NameSeparator ?? o2?.NameSeparator,
            TargetName = o1?.TargetName ?? o2?.TargetName,
            TargetType = o1?.TargetType ?? o2?.TargetType,
            UseQualifiedName = o1?.UseQualifiedName ?? o2?.UseQualifiedName
        };
        return result;
    }

    /// <summary>
    /// One or more custom fields to bind.
    /// </summary>
    /// <remarks>
    /// See <see href="https://aka.ms/ps-rule/options#bindingfield"/>.
    /// </remarks>
    [DefaultValue(null)]
    public FieldMap Field { get; set; }

    /// <summary>
    /// Determines if custom binding uses ignores case when matching properties.
    /// </summary>
    /// <remarks>
    /// See <see href="https://aka.ms/ps-rule/options#bindingignorecase"/>.
    /// </remarks>
    [DefaultValue(null)]
    public bool? IgnoreCase { get; set; }

    /// <summary>
    /// Configures the separator to use for building a qualified name.
    /// </summary>
    /// <remarks>
    /// See <see href="https://aka.ms/ps-rule/options#bindingnameseparator"/>.
    /// </remarks>
    [DefaultValue(null)]
    public string NameSeparator { get; set; }

    /// <summary>
    /// Property names to use to bind TargetName.
    /// </summary>
    /// <remarks>
    /// See <see href="https://aka.ms/ps-rule/options#bindingtargetname"/>.
    /// </remarks>
    [DefaultValue(null)]
    public string[] TargetName { get; set; }

    /// <summary>
    /// Property names to use to bind TargetType.
    /// </summary>
    /// <remarks>
    /// See <see href="https://aka.ms/ps-rule/options#bindingtargettype"/>.
    /// </remarks>
    [DefaultValue(null)]
    public string[] TargetType { get; set; }

    /// <summary>
    /// Determines if a qualified TargetName is used.
    /// </summary>
    /// <remarks>
    /// See <see href="https://aka.ms/ps-rule/options#bindingusequalifiedname"/>.
    /// </remarks>
    [DefaultValue(null)]
    public bool? UseQualifiedName { get; set; }

    /// <summary>
    /// Load from environment variables.
    /// </summary>
    internal void Load()
    {
        // Binding.Field - currently not supported

        if (Environment.TryBool("PSRULE_BINDING_IGNORECASE", out var ignoreCase))
            IgnoreCase = ignoreCase;

        if (Environment.TryString("PSRULE_BINDING_NAMESEPARATOR", out var nameSeparator))
            NameSeparator = nameSeparator;

        if (Environment.TryStringArray("PSRULE_BINDING_TARGETNAME", out var targetName))
            TargetName = targetName;

        if (Environment.TryStringArray("PSRULE_BINDING_TARGETTYPE", out var targetType))
            TargetType = targetType;

        if (Environment.TryBool("PSRULE_BINDING_USEQUALIFIEDNAME", out var useQualifiedName))
            UseQualifiedName = useQualifiedName;
    }

    /// <inheritdoc/>
    public void Import(IDictionary<string, object> dictionary)
    {
        if (dictionary.TryPopValue("Binding.Field", out Hashtable map))
            Field = new FieldMap(map);

        if (dictionary.TryPopBool("Binding.IgnoreCase", out var ignoreCase))
            IgnoreCase = ignoreCase;

        if (dictionary.TryPopString("Binding.NameSeparator", out var nameSeparator))
            NameSeparator = nameSeparator;

        if (dictionary.TryPopStringArray("Binding.TargetName", out var targetName))
            TargetName = targetName;

        if (dictionary.TryPopStringArray("Binding.TargetType", out var targetType))
            TargetType = targetType;

        if (dictionary.TryPopValue("Binding.UseQualifiedName", out bool useQualifiedName))
            UseQualifiedName = useQualifiedName;
    }
}
