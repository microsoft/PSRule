// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using PSRule.Definitions.Baselines;
using PSRule.Options;

namespace PSRule.Configuration;

/// <summary>
/// A subset of options that can be defined within a baseline.
/// These options can be passes as a baseline for use within a pipeline.
/// </summary>
public class BaselineOption
{
    internal sealed class BaselineRef(string name) : BaselineOption
    {
        public readonly string Name = name;
    }

    internal sealed class BaselineInline() : BaselineOption, IBaselineV1Spec
    {
        public ConfigurationOption Configuration { get; set; } = [];

        public OverrideOption Override { get; set; }

        public RuleOption Rule { get; set; } = new RuleOption();
    }

    /// <summary>
    /// Creates a baseline option from a hashtable of key/ values.
    /// </summary>
    /// <param name="hashtable">A hashtable of key/ values.</param>
    /// <returns>A baseline option composed of provided key/ values.</returns>
    public static implicit operator BaselineOption(Hashtable hashtable)
    {
        return FromHashtable(hashtable);
    }

    /// <summary>
    /// Creates a reference to a baseline by name which is resolved at runtime.
    /// </summary>
    /// <param name="value">The name of the baseline.</param>
    /// <returns>A reference to a baseline option.</returns>
    public static implicit operator BaselineOption(string value)
    {
        return FromString(value);
    }

    /// <summary>
    /// Creates a baseline option from a hashtable of key/ values.
    /// </summary>
    /// <param name="hashtable">A hashtable of key/ values.</param>
    /// <returns>A baseline option composed of provided key/ values.</returns>
    public static BaselineOption FromHashtable(Hashtable hashtable)
    {
        var option = new BaselineInline();
        if (hashtable != null)
        {
            // Build index to allow mapping
            var index = PSRuleOption.BuildIndex(hashtable);
            Load(option, index);
        }
        return option;
    }

    /// <summary>
    /// Creates a reference to a baseline by name which is resolved at runtime.
    /// </summary>
    /// <param name="value">The name of the baseline.</param>
    /// <returns>A reference to a baseline option.</returns>
    public static BaselineOption? FromString(string? value)
    {
        return value == null || string.IsNullOrEmpty(value) ? null : new BaselineRef(value);
    }

    /// <summary>
    /// Load from environment variables.
    /// </summary>
    internal static void Load(IBaselineV1Spec option)
    {
        if (Environment.TryResourceIdReference("PSRULE_RULE_BASELINE", out var baseline))
            option.Rule.Baseline = baseline;

        if (Environment.TryResourceIdReferenceArray("PSRULE_RULE_EXCLUDE", out var exclude))
            option.Rule.Exclude = exclude;

        if (Environment.TryBool("PSRULE_RULE_INCLUDELOCAL", out var includeLocal))
            option.Rule.IncludeLocal = includeLocal;

        if (Environment.TryResourceIdReferenceArray("PSRULE_RULE_INCLUDE", out var include))
            option.Rule.Include = include;

        // Rule.Tag - currently not supported

        // Process configuration values
        option.Override.Load();
        option.Configuration.Load();
    }

    /// <summary>
    /// Load from a dictionary.
    /// </summary>
    internal static void Load(IBaselineV1Spec option, Dictionary<string, object> properties)
    {
        if (properties.TryPopResourceIdReference("Rule.Baseline", out var baseline))
            option.Rule.Baseline = baseline;

        if (properties.TryPopResourceIdReferenceArray("Rule.Exclude", out var exclude))
            option.Rule.Exclude = exclude;

        if (properties.TryPopBool("Rule.IncludeLocal", out var includeLocal))
            option.Rule.IncludeLocal = includeLocal;

        if (properties.TryPopResourceIdReferenceArray("Rule.Include", out var include))
            option.Rule.Include = include;

        if (properties.TryPopValue("Rule.Tag", out Hashtable? tag) && tag != null)
            option.Rule.Tag = tag;

        // Process configuration values
        option.Override.Import(properties);
        option.Configuration.Load(properties);
    }
}
