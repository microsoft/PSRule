// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.ComponentModel;
using PSRule.Definitions;

namespace PSRule.Configuration;

/// <summary>
/// Options for that affect which rules are executed by including and filtering discovered rules.
/// </summary>
public sealed class RuleOption : IEquatable<RuleOption>
{
    private const bool DEFAULT_INCLUDE_LOCAL = false;

    internal static readonly RuleOption Default = new()
    {
        IncludeLocal = DEFAULT_INCLUDE_LOCAL
    };

    /// <summary>
    /// Create an empty rule option.
    /// </summary>
    public RuleOption()
    {
        Baseline = null;
        Exclude = null;
        IncludeLocal = null;
        Include = null;
        Tag = null;
        Labels = null;
    }

    /// <summary>
    /// Create a rule option by copying an existing instance.
    /// </summary>
    /// <param name="option">The option instance to copy.</param>
    public RuleOption(RuleOption? option)
    {
        if (option == null)
            return;

        Baseline = option.Baseline;
        Exclude = option.Exclude;
        IncludeLocal = option.IncludeLocal;
        Include = option.Include;
        Tag = option.Tag;
        Labels = option.Labels;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is RuleOption option && Equals(option);
    }

    /// <inheritdoc/>
    public bool Equals(RuleOption other)
    {
        return other != null &&
            Baseline == other.Baseline &&
            Exclude == other.Exclude &&
            IncludeLocal == other.IncludeLocal &&
            Include == other.Include &&
            Tag == other.Tag &&
            Labels == other.Labels;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked // Overflow is fine
        {
            var hash = 17;
            hash = hash * 23 + (Baseline != null ? Baseline.GetHashCode() : 0);
            hash = hash * 23 + (Exclude != null ? Exclude.GetHashCode() : 0);
            hash = hash * 23 + (IncludeLocal.HasValue ? IncludeLocal.Value.GetHashCode() : 0);
            hash = hash * 23 + (Include != null ? Include.GetHashCode() : 0);
            hash = hash * 23 + (Tag != null ? Tag.GetHashCode() : 0);
            hash = hash * 23 + (Labels != null ? Labels.GetHashCode() : 0);
            return hash;
        }
    }

    /// <summary>
    /// Merge two option instances by replacing any unset properties from <paramref name="o1"/> with <paramref name="o2"/> values.
    /// Values from <paramref name="o1"/> that are set are not overridden.
    /// </summary>
    internal static RuleOption Combine(RuleOption? o1, RuleOption? o2)
    {
        var result = new RuleOption(o1)
        {
            Baseline = o1?.Baseline ?? o2?.Baseline,
            Exclude = o1?.Exclude ?? o2?.Exclude,
            IncludeLocal = o1?.IncludeLocal ?? o2?.IncludeLocal,
            Include = o1?.Include ?? o2?.Include,
            Tag = o1?.Tag ?? o2?.Tag,
            Labels = o1?.Labels ?? o2?.Labels,
        };
        return result;
    }

    /// <summary>
    /// The name of a baseline to use.
    /// </summary>
    [DefaultValue(null)]
    public ResourceIdReference? Baseline { get; set; }

    /// <summary>
    /// A set of rules to exclude for execution.
    /// </summary>
    [DefaultValue(null)]
    public ResourceIdReference[]? Exclude { get; set; }

    /// <summary>
    /// Automatically include all local rules in the search path unless they have been explicitly excluded.
    /// </summary>
    [DefaultValue(null)]
    public bool? IncludeLocal { get; set; }

    /// <summary>
    /// A set of rules to include for execution.
    /// </summary>
    [DefaultValue(null)]
    public ResourceIdReference[]? Include { get; set; }

    /// <summary>
    /// A set of rule tags to include for execution.
    /// </summary>
    [DefaultValue(null)]
    public Hashtable Tag { get; set; }

    /// <summary>
    /// A set of taxonomy references.
    /// </summary>
    [DefaultValue(null)]
    public ResourceLabels Labels { get; set; }
}
