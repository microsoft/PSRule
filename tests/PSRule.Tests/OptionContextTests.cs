// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.Rules;
using PSRule.Pipeline;

namespace PSRule
{
    /// <summary>
    /// Tests for <see cref="OptionContext"/>.
    /// </summary>
    public sealed class OptionContextTests
    {
        [Fact]
        public void Build()
        {
            // Create option context
            var builder = new OptionContextBuilder(GetOption());
            var optionContext = builder.Build();

            Assert.NotNull(optionContext);

            // Check empty scope
            var testScope = new Runtime.LanguageScope(null, "Empty");
            optionContext.UpdateLanguageScope(testScope);
            Assert.Equal(new string[] { "en-ZZ" }, testScope.Culture);
        }

        [Fact]
        public void Order()
        {
            // Create option context
            var builder = new OptionContextBuilder(GetOption());
            var optionContext = builder.Build();

            var localScope = new Runtime.LanguageScope(null, null);
            optionContext.UpdateLanguageScope(localScope);

            var ruleFilter = localScope.GetFilter(ResourceKind.Rule) as RuleFilter;
            Assert.NotNull(ruleFilter);
            Assert.True(ruleFilter.IncludeLocal);

            // With explict baseline
            builder = new OptionContextBuilder(GetOption());
            optionContext = builder.Build();
            optionContext.Add(new OptionContext.BaselineScope(OptionContext.ScopeType.Explicit, new string[] {"abc" }, null, null));
            optionContext.UpdateLanguageScope(localScope);
            ruleFilter = localScope.GetFilter(ResourceKind.Rule) as RuleFilter;
            Assert.NotNull(ruleFilter);
            Assert.False(ruleFilter.IncludeLocal);

            // With include from parameters
            builder = new OptionContextBuilder(GetOption(), include: new string[] { "abc" });
            optionContext = builder.Build();
            optionContext.UpdateLanguageScope(localScope);
            ruleFilter = localScope.GetFilter(ResourceKind.Rule) as RuleFilter;
            Assert.NotNull(ruleFilter);
            Assert.False(ruleFilter.IncludeLocal);

            builder = new OptionContextBuilder(GetOption());
            optionContext = builder.Build();
            optionContext.Add(new OptionContext.BaselineScope(OptionContext.ScopeType.Workspace, new string[] { "abc" }, null, null));
            optionContext.UpdateLanguageScope(localScope);
            ruleFilter = localScope.GetFilter(ResourceKind.Rule) as RuleFilter;
            Assert.NotNull(ruleFilter);
            Assert.True(ruleFilter.IncludeLocal);
        }

        #region Helper methods

        internal sealed class MockScope : Runtime.ILanguageScope
        {
            internal RuleFilter RuleFilter;

            public MockScope(string name)
            {
                Name = name;
            }

            public string Name { get; }

            public IBindingOption Binding => throw new System.NotImplementedException();

            public string[] Culture => throw new System.NotImplementedException();

            public void AddService(string name, object service)
            {
                
            }

            public void Configure(Dictionary<string, object> configuration)
            {
                
            }

            public void Dispose()
            {
                
            }

            public IResourceFilter GetFilter(ResourceKind kind)
            {
                throw new System.NotImplementedException();
            }

            public object GetService(string name)
            {
                throw new System.NotImplementedException();
            }

            public bool TryConfigurationValue(string key, out object value)
            {
                throw new System.NotImplementedException();
            }

            public bool TryGetName(object o, out string name, out string path)
            {
                throw new System.NotImplementedException();
            }

            public bool TryGetScope(object o, out string[] scope)
            {
                throw new System.NotImplementedException();
            }

            public bool TryGetType(object o, out string type, out string path)
            {
                throw new System.NotImplementedException();
            }

            public void WithBinding(IBindingOption bindingOption)
            {
                
            }

            public void WithCulture(string[] strings)
            {
                
            }

            public void WithFilter(IResourceFilter resourceFilter)
            {
                if (resourceFilter is RuleFilter ruleFilter)
                    RuleFilter = ruleFilter;
            }
        }

        private static PSRuleOption GetOption(string[] culture = null)
        {
            var option = new PSRuleOption();

            // Specify a culture otherwise it varies within CI.
            option.Output.Culture = culture ?? new string[] { "en-ZZ" };
            return option;
        }

        #endregion Helper methods
    }
}
