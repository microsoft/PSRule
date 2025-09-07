// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Options;
using PSRule.Rules;

namespace PSRule.Pipeline.Runs;

/// <summary>
/// A builder to create a <see cref="RunCollection"/>.
/// </summary>
internal sealed class RunCollectionBuilder(PSRuleOption? option, string instance)
{
    private const char SLASH = '/';
    private const char SPACE = ' ';
    private const char DOT = '.';

    private readonly string _Category = NormalizeCategory(option?.Run?.Category);
    private readonly string _Description = option?.Run?.Description ?? RunOption.Default.Description!;
    private readonly string _Instance = instance ?? throw new ArgumentNullException(nameof(instance));

    /// <summary>
    /// A correlation identifier for all related runs.
    /// </summary>
    private readonly string _CorrelationGuid = Guid.NewGuid().ToString();

    private readonly RunCollection _Runs = [];

    /// <summary>
    /// Build a <see cref="RunCollection"/>.
    /// </summary>
    public RunCollection Build()
    {
        return _Runs;
    }

    public RunCollectionBuilder WithDefaultRun(DependencyGraph<IRuleBlock> graph)
    {
        _Runs.Add(new Run(
            id: NormalizeId(_Category, string.Empty, _Instance),
            description: new InfoString(_Description),
            correlationGuid: _CorrelationGuid,
            graph: new RuleGraph(graph)
        ));

        return this;
    }

    /// <summary>
    /// Trim out any leading or trailing whitespace, slashes, or dots.
    /// </summary>
    private static string NormalizeCategory(string? category)
    {
        var result = category?.TrimStart(SPACE, SLASH)?.TrimEnd(SPACE, SLASH, DOT);
        return string.IsNullOrWhiteSpace(result) ? RunOption.Default.Category! : result!;
    }

    /// <summary>
    /// Normalize the run identifier to remove segments that are not required.
    /// For example: <c>NormalizeId("Category", "Name", "Instance") => "Category/Name/Instance"</c>
    /// </summary>
    /// <param name="category">The category of the run.</param>
    /// <param name="name">An optional name of the run. The name is ignored if it is empty, whitespace, or <c>.</c>.</param>
    /// <param name="instance">The instance of the run.</param>
    /// <returns>A formatted string with each segment separated by a <c>/</c>.</returns>
    private static string NormalizeId(string category, string name, string instance)
    {
        return name == "." || string.IsNullOrWhiteSpace(name)
            ? string.Concat(category, SLASH, instance)
            : string.Concat(category, SLASH, name, SLASH, instance);
    }
}
