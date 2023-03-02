// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Host;
using Xunit;

namespace PSRule
{
    public sealed class LanguageVisitorTests
    {
        [Fact]
        public void NestedRule()
        {
            var content = @"
# Header comment
Rule 'Rule1' {

}
";
            var scriptAst = ScriptBlock.Create(content).Ast;
            var visitor = new RuleLanguageAst();
            scriptAst.Visit(visitor);

            Assert.Null(visitor.Errors);

            content = @"
# Header comment
Rule 'Rule1' {
    Rule 'Rule2' {

    }
}
";
            scriptAst = ScriptBlock.Create(content).Ast;
            visitor = new RuleLanguageAst();
            scriptAst.Visit(visitor);

            Assert.Single(visitor.Errors);
        }

        [Fact]
        public void UnvalidRule()
        {
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
            var visitor = new RuleLanguageAst();
            scriptAst.Visit(visitor);

            Assert.NotNull(visitor.Errors);
            Assert.Equal(4, visitor.Errors.Count);
        }
    }
}
