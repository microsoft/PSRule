// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Host;

namespace PSRule;

public sealed class RuleLanguageAstTests : BaseTests
{
    [Fact]
    public void RuleName()
    {
        var logger = GetTestWriter();
        var scriptAst = System.Management.Automation.Language.Parser.ParseFile(GetSourcePath("FromFileName.Rule.ps1"), out _, out _);
        var visitor = new RuleLanguageAst(logger);
        scriptAst.Visit(visitor);

        Assert.Equal("PSR0018", logger.Errors[0].eventId.Name);
    }
}
