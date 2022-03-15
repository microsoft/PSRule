// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using PSRule.Host;
using Xunit;

namespace PSRule
{
    public sealed class RuleLanguageAstTests
    {
        [Fact]
        public void RuleName()
        {
            var scriptAst = System.Management.Automation.Language.Parser.ParseFile(GetSourcePath("FromFileName.Rule.ps1"), out _, out _);
            var visitor = new RuleLanguageAst(null);
            scriptAst.Visit(visitor);

            Assert.Equal("PSRule.Parse.InvalidResourceName", visitor.Errors[0].FullyQualifiedErrorId);
        }

        #region Helper methods

        private static string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        #endregion Helper methods
    }
}
