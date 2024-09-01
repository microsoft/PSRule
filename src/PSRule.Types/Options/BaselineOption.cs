// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using PSRule.Data;

namespace PSRule.Options;

/// <summary>
/// Options that configure baselines.
/// </summary>
/// <remarks>
/// See <see href="https://aka.ms/ps-rule/options"/>.
/// </remarks>
public sealed class BaselineOption : IEquatable<BaselineOption>, IBaselineOption
{
    internal static readonly BaselineOption Default = new()
    {
    };

    /// <summary>
    /// Create an option instance.
    /// </summary>
    public BaselineOption()
    {
        Group = null;
    }

    /// <summary>
    /// Create an option instance based on an existing object.
    /// </summary>
    /// <param name="option">The existing object to copy.</param>
    public BaselineOption(BaselineOption option)
    {
        if (option == null)
            return;

        Group = option.Group;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is BaselineOption option && Equals(option);
    }

    /// <inheritdoc/>
    public bool Equals(BaselineOption other)
    {
        return other != null &&
            Group == other.Group;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked // Overflow is fine
        {
            var hash = 17;
            hash = hash * 23 + (Group != null ? Group.GetHashCode() : 0);
            return hash;
        }
    }

    /// <summary>
    /// Combines two option instances into a new merged instance.
    /// The new instance uses any non-null values from <paramref name="o1"/>.
    /// Any null values from <paramref name="o1"/> are replaced with <paramref name="o2"/>.
    /// </summary>
    public static BaselineOption Combine(BaselineOption o1, BaselineOption o2)
    {
        var result = new BaselineOption(o1)
        {
            Group = o1?.Group ?? o2?.Group,
        };
        return result;
    }

    /// <inheritdoc/>
    [DefaultValue(null)]
    public StringArrayMap? Group { get; set; }

    /// <summary>
    /// Load from environment variables.
    /// </summary>
    internal void Load()
    {
        if (Environment.TryStringArrayMap("PSRULE_BASELINE_GROUP", out var group))
            Group = group;
    }

    /// <summary>
    /// Load from a dictionary.
    /// </summary>
    internal void Load(Dictionary<string, object> index)
    {
        if (index.TryPopStringArrayMap("Baseline.Group", out var group))
            Group = group;
    }
}
