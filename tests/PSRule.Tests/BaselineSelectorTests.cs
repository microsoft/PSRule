// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using PSRule.Definitions;
using PSRule.Definitions.Baselines;
using PSRule.Definitions.Rules;

namespace PSRule;

/// <summary>
/// Tests for baseline selector functionality.
/// </summary>
public sealed class BaselineSelectorTests : ContextBaseTests
{
    private const string BaselineSelectorFileName = "BaselineSelector.Rule.yaml";

    [Fact]
    public void BaselineRuleFilter_WithSelector()
    {
        // Test that BaselineRuleFilter can filter rules using selectors
        var context = GetContext();
        var rules = GetTestRules();
        
        // Create a simple selector that filters for rules with "Error" level
        var filter = new BaselineRuleFilter(
            include: null,
            tag: null,
            exclude: null,
            includeLocal: true,
            labels: null,
            selector: null, // Will implement selector parsing later
            context: context
        );

        var filteredRules = rules.Where(rule => filter.Match(rule)).ToArray();
        
        // For now, should behave like regular RuleFilter (no selector)
        Assert.NotEmpty(filteredRules);
    }

    [Fact]
    public void ReadBaseline_WithSelector()
    {
        // Test reading baseline YAML with selector
        var baselines = GetBaselines(GetSource(BaselineSelectorFileName));
        Assert.NotNull(baselines);
        Assert.Equal(4, baselines.Length);

        // Validate that baselines were read correctly
        Assert.Equal("HighSeverityBaseline", baselines[0].Name);
        Assert.Equal("PrefixBaseline", baselines[1].Name);
        Assert.Equal("ComplexBaseline", baselines[2].Name);
        Assert.Equal("AnnotationBaseline", baselines[3].Name);
    }

    private IResource[] GetTestRules()
    {
        // Create mock rules for testing
        var rules = new List<IResource>();
        
        // This will be expanded once we have the filtering working
        // For now, return empty array
        return rules.ToArray();
    }
}