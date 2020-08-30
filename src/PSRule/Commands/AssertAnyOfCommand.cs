// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using PSRule.Runtime;
using System.Management.Automation;

namespace PSRule.Commands
{
    /// <summary>
    /// The AnyOf keyword.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Assert, RuleLanguageNouns.AnyOf)]
    internal sealed class AssertAnyOfCommand : RuleKeyword
    {
        [Parameter(Mandatory = true, Position = 0)]
        public ScriptBlock Body { get; set; }

        protected override void ProcessRecord()
        {
            if (!IsConditionScope())
                throw ConditionScopeException(LanguageKeywords.AnyOf);

            var invokeResult = RuleConditionHelper.Create(Body.Invoke());
            var result = invokeResult.AnyOf();

            RunspaceContext.CurrentThread.VerboseConditionResult(condition: RuleLanguageNouns.AnyOf, pass: invokeResult.Pass, count: invokeResult.Count, outcome: result);
            WriteObject(result);
        }
    }
}
