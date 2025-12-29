// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using PSRule.Definitions;
using PSRule.Definitions.Rules;

namespace PSRule;

/// <summary>
/// Define tests to validate <see cref="RuleFilter"/>.
/// </summary>
public sealed partial class RuleFilterTests
{
    [Fact]
    public void MatchInclude()
    {
        var filter = new RuleFilter(["rule1", "rule2"], null, null, null, null);
        Assert.True(filter.Match(GetResource("rule1")));
        Assert.True(filter.Match(GetResource("Rule2")));
        Assert.False(filter.Match(GetResource("rule3")));
    }

    [Fact]
    public void MatchExclude()
    {
        var filter = new RuleFilter(null, null, ["rule3"], null, null);
        Assert.True(filter.Match(GetResource("rule1")));
        Assert.True(filter.Match(GetResource("rule2")));
        Assert.False(filter.Match(GetResource("Rule3")));
    }

    [Fact]
    public void MatchTag()
    {
        // Set resource tags
        var resourceTags = new Hashtable();

        // Create a filter with category equal to group 1 or group 2.
        var tag = new Hashtable
        {
            ["category"] = new string[] { "group1", "group2" }
        };
        var filter = new RuleFilter(null, tag, null, null, null);

        // Check basic match
        resourceTags["category"] = "group2";
        Assert.True(filter.Match(GetResource("rule", ResourceTags.FromHashtable(resourceTags))));
        resourceTags["category"] = "group1";
        Assert.True(filter.Match(GetResource("rule", ResourceTags.FromHashtable(resourceTags))));
        resourceTags["category"] = "group3";
        Assert.False(filter.Match(GetResource("rule", ResourceTags.FromHashtable(resourceTags))));
        Assert.False(filter.Match(GetResource("rule")));

        // Include local
        filter = new RuleFilter(null, tag, null, true, null);
        resourceTags["category"] = "group1";
        Assert.True(filter.Match(GetResource("module1\\rule", ResourceTags.FromHashtable(resourceTags))));
        resourceTags["category"] = "group3";
        Assert.False(filter.Match(GetResource("module1\\rule", ResourceTags.FromHashtable(resourceTags))));
        resourceTags["category"] = "group3";
        Assert.True(filter.Match(GetResource(".\\rule", ResourceTags.FromHashtable(resourceTags))));
        Assert.False(filter.Match(GetResource("module1\\rule")));
        Assert.True(filter.Match(GetResource(".\\rule")));
    }

    [Fact]
    public void MatchLabels()
    {
        // Set resource tags
        var resourceLabels = new Hashtable();

        // Create a filter
        var labels = new ResourceLabels
        {
            ["framework.v1/control"] = ["c-1", "c-2"]
        };
        var filter = new RuleFilter(null, null, null, null, labels);

        resourceLabels["framework.v1/control"] = new string[] { "c-2", "c-1" };
        Assert.True(filter.Match(GetResource("rule", null, ResourceLabels.FromHashtable(resourceLabels))));
        resourceLabels["framework.v1/control"] = new string[] { "c-3", "c-1" };
        Assert.True(filter.Match(GetResource("rule", null, ResourceLabels.FromHashtable(resourceLabels))));
        resourceLabels["framework.v1/control"] = new string[] { "c-1", "c-3" };
        Assert.True(filter.Match(GetResource("rule", null, ResourceLabels.FromHashtable(resourceLabels))));
        resourceLabels["framework.v1/control"] = new string[] { "c-3", "c-4" };
        Assert.False(filter.Match(GetResource("rule", null, ResourceLabels.FromHashtable(resourceLabels))));
        resourceLabels["framework.v1/control"] = Array.Empty<string>();
        Assert.False(filter.Match(GetResource("rule", null, ResourceLabels.FromHashtable(resourceLabels))));
    }

    #region Helper Methods

    private static IResource GetResource(string id, ResourceTags? resourceTags = null, ResourceLabels? resourceLabels = null)
    {
        return new TestResourceName(ResourceId.Parse(id), resourceTags, resourceLabels);
    }

    #endregion Helper Methods
}
