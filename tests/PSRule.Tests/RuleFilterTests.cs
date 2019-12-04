// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Rules;
using System.Collections;
using Xunit;

namespace PSRule
{
    public sealed class RuleFilterTests
    {
        [Fact]
        public void MatchInclude()
        {
            var filter = new RuleFilter(new string[] { "rule1", "rule2" }, null, null);
            Assert.True(filter.Match("rule1", null));
            Assert.True(filter.Match("Rule2", null));
            Assert.False(filter.Match("rule3", null));
        }

        [Fact]
        public void MatchExclude()
        {
            var filter = new RuleFilter(null, null, new string[] { "rule3" });
            Assert.True(filter.Match("rule1", null));
            Assert.True(filter.Match("rule2", null));
            Assert.False(filter.Match("Rule3", null));
        }

        [Fact]
        public void MatchTag()
        {
            var tag = new Hashtable();
            tag["category"] = new string[] { "group1", "group2" };
            var filter = new RuleFilter(null, tag, null);

            var ruleTags = new Hashtable();
            ruleTags["category"] = "group2";
            Assert.True(filter.Match("rule1", TagSet.FromHashtable(ruleTags)));
            ruleTags["category"] = "group1";
            Assert.True(filter.Match("rule2", TagSet.FromHashtable(ruleTags)));
            ruleTags["category"] = "group3";
            Assert.False(filter.Match("rule3", TagSet.FromHashtable(ruleTags)));
            Assert.False(filter.Match("rule4", null));
        }
    }
}
