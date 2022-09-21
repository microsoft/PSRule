// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using PSRule.Definitions;
using PSRule.Definitions.Rules;
using Xunit;

namespace PSRule
{
    /// <summary>
    /// Define tests to validate <see cref="RuleFilter"/>.
    /// </summary>
    public sealed class RuleFilterTests
    {
        [Fact]
        public void MatchInclude()
        {
            var filter = new RuleFilter(new string[] { "rule1", "rule2" }, null, null, null, null);
            Assert.True(filter.Match("rule1", null, null));
            Assert.True(filter.Match("Rule2", null, null));
            Assert.False(filter.Match("rule3", null, null));
        }

        [Fact]
        public void MatchExclude()
        {
            var filter = new RuleFilter(null, null, new string[] { "rule3" }, null, null);
            Assert.True(filter.Match("rule1", null, null));
            Assert.True(filter.Match("rule2", null, null));
            Assert.False(filter.Match("Rule3", null, null));
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
            Assert.True(filter.Match("rule", ResourceTags.FromHashtable(resourceTags), null));
            resourceTags["category"] = "group1";
            Assert.True(filter.Match("rule", ResourceTags.FromHashtable(resourceTags), null));
            resourceTags["category"] = "group3";
            Assert.False(filter.Match("rule", ResourceTags.FromHashtable(resourceTags), null));
            Assert.False(filter.Match("rule", null, null));
        }

        [Fact]
        public void MatchTaxa()
        {
            // Set resource tags
            var resourceTaxa = new Hashtable();

            // Create a filter
            var taxa = new ResourceTaxa
            {
                ["framework.v1/control"] = new string[] { "c-1", "c-2" }
            };
            var filter = new RuleFilter(null, null, null, null, taxa);

            resourceTaxa["framework.v1/control"] = new string[] { "c-2", "c-1" };
            Assert.True(filter.Match("rule", null, ResourceTaxa.FromHashtable(resourceTaxa)));
            resourceTaxa["framework.v1/control"] = new string[] { "c-3", "c-1" };
            Assert.True(filter.Match("rule", null, ResourceTaxa.FromHashtable(resourceTaxa)));
            resourceTaxa["framework.v1/control"] = new string[] { "c-1", "c-3" };
            Assert.True(filter.Match("rule", null, ResourceTaxa.FromHashtable(resourceTaxa)));
            resourceTaxa["framework.v1/control"] = new string[] { "c-3", "c-4" };
            Assert.False(filter.Match("rule", null, ResourceTaxa.FromHashtable(resourceTaxa)));
            resourceTaxa["framework.v1/control"] = System.Array.Empty<string>();
            Assert.False(filter.Match("rule", null, ResourceTaxa.FromHashtable(resourceTaxa)));
        }
    }
}
