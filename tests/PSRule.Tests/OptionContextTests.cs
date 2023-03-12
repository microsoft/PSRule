// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Pipeline;
using Xunit;

namespace PSRule
{
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

        #region Helper methods

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
