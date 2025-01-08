// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;

namespace PSRule.Options;

/// <summary>
/// Options that configure required capabilities.
/// </summary>
/// <remarks>
/// See <see href="https://aka.ms/ps-rule/options"/>.
/// </remarks>
public sealed class CapabilityOption : ICapabilityOption, IEquatable<CapabilityOption>
{
    /// <summary>
    /// The default capability option.
    /// </summary>
    public static readonly CapabilityOption Default = new()
    {

    };

    /// <summary>
    /// Creates an empty capability option.
    /// </summary>
    public CapabilityOption()
    {

    }

    /// <summary>
    /// Creates a capability option by copying an existing instance.
    /// </summary>
    /// <param name="option"></param>
    public CapabilityOption(CapabilityOption? option)
    {
        if (option == null)
            return;

        Items = option.Items;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is CapabilityOption option && Equals(option);
    }

    /// <inheritdoc/>
    public bool Equals(CapabilityOption? other)
    {
        return other != null &&
            Items == other.Items;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked // Overflow is fine
        {
            var hash = 17;
            hash = hash * 23 + (Items != null ? Items.GetHashCode() : 0);
            return hash;
        }
    }

    /// <summary>
    /// Merge two option instances by replacing any unset properties from <paramref name="o1"/> with <paramref name="o2"/> values.
    /// Values from <paramref name="o1"/> that are set are not overridden.
    /// </summary>
    internal static CapabilityOption Combine(CapabilityOption? o1, CapabilityOption? o2)
    {
        var result = new CapabilityOption(o1)
        {
            Items = o1?.Items ?? o2?.Items,
        };
        return result;
    }

    /// <summary>
    /// A list of required capabilities.
    /// </summary>
    [DefaultValue(null)]
    public string[]? Items { get; set; }

    /// <summary>
    /// Load from environment variables.
    /// </summary>
    internal void Load()
    {
        if (Environment.TryStringArray("PSRULE_CAPABILITIES", out var items))
            Items = items;
    }

    /// <inheritdoc/>
    public void Import(IDictionary<string, object> dictionary)
    {
        if (dictionary.TryPopStringArray("Capabilities", out var items))
            Items = items;
    }
}

/// <summary>
/// Options that configure required capabilities.
/// </summary>
public interface ICapabilityOption : IOption
{
    /// <summary>
    /// A list of required capabilities.
    /// </summary>
    string[]? Items { get; }
}
