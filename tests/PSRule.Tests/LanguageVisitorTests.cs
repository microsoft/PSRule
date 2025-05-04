// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Host;

namespace PSRule;

public sealed class LanguageVisitorTests : ContextBaseTests
{
    [Fact]
    public void NestedRule()
    {
        var logger = GetTestWriter();
        var content = @"
# Header comment
Rule 'Rule1' {

}
";
        var scriptAst = ScriptBlock.Create(content).Ast;
        var visitor = new RuleLanguageAst(logger);
        scriptAst.Visit(visitor);

        Assert.Empty(logger.Errors);

        logger = GetTestWriter();
        content = @"
# Header comment
Rule 'Rule1' {
    Rule 'Rule2' {

    }
}
";
        scriptAst = ScriptBlock.Create(content).Ast;
        visitor = new RuleLanguageAst(logger);
        scriptAst.Visit(visitor);

        Assert.Single(logger.Errors);
    }

    [Fact]
    public void UnvalidRule()
    {
        var logger = GetTestWriter();
        var content = @"
Rule '' {

}

Rule {

}

Rule 'Rule1';

Rule '' {

}

Rule 'Rule2' {

}

Rule -Name 'Rule3' {

}

Rule -Name 'Rule3' -Body {

}

";

        var scriptAst = ScriptBlock.Create(content).Ast;
        var visitor = new RuleLanguageAst(logger);
        scriptAst.Visit(visitor);

        Assert.NotNull(logger.Errors);
        Assert.Equal(4, logger.Errors.Count);
    }
}
