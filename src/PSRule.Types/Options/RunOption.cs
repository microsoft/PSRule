// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System.ComponentModel;

namespace PSRule.Options;

/// <summary>
/// Options that configure runs.
/// </summary>
/// <remarks>
/// See <see href="https://aka.ms/ps-rule/options"/>.
/// </remarks>
public sealed class RunOption : IRunOption, IEquatable<RunOption>
{
    private const string DEFAULT_CATEGORY = "PSRule";

    /// <summary>
    /// The default run option.
    /// </summary>
    public static readonly RunOption Default = new()
    {
        Category = DEFAULT_CATEGORY,
        Description = string.Empty,
    };

    /// <summary>
    /// Creates an empty run option.
    /// </summary>
    public RunOption()
    {
        Category = null;
        Description = null;
    }

    /// <summary>
    /// Creates a run option by copying an existing instance.
    /// </summary>
    /// <param name="option">The option instance to copy.</param>
    public RunOption(RunOption? option)
    {
        if (option == null)
            return;

        Category = option.Category;
        Description = option.Description;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is RunOption option && Equals(option);
    }

    /// <inheritdoc/>
    public bool Equals(RunOption? other)
    {
        return other != null &&
            Category == other.Category &&
            Description == other.Description;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked // Overflow is fine
        {
            var hash = 17;
            hash = hash * 23 + (Category != null ? Category.GetHashCode() : 0);
            hash = hash * 23 + (Description != null ? Description.GetHashCode() : 0);
            return hash;
        }
    }

    /// <summary>
    /// Merge two option instances by replacing any unset properties from <paramref name="o1"/> with <paramref name="o2"/> values.
    /// Values from <paramref name="o1"/> that are set are not overridden.
    /// </summary>
    internal static RunOption Combine(RunOption? o1, RunOption? o2)
    {
        var result = new RunOption(o1)
        {
            Category = o1?.Category ?? o2?.Category,
            Description = o1?.Description ?? o2?.Description,
        };
        return result;
    }

    /// <summary>
    /// Configures the run category that is used as an identifier for output results.
    /// </summary>
    [DefaultValue(null)]
    public string? Category { get; set; }

    /// <summary>
    /// Configure the run description that is displayed in output.
    /// </summary>

    [DefaultValue(null)]
    public string? Description { get; set; }

    /// <summary>
    /// Load from environment variables.
    /// </summary>
    internal void Load()
    {
        if (Environment.TryString("PSRULE_RUN_CATEGORY", out var category))
            Category = category;

        if (Environment.TryString("PSRULE_RUN_DESCRIPTION", out var description))
            Description = description;
    }

    /// <inheritdoc/>
    public void Import(IDictionary<string, object> dictionary)
    {
        if (dictionary.TryPopString("Run.Category", out var category))
            Category = category;

        if (dictionary.TryPopString("Run.Description", out var description))
            Description = description;
    }
}
