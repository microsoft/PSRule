// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Runtime;
using System.Management.Automation;
using System.Threading;

namespace PSRule.Commands
{
    /// <summary>
    /// The AllOf keyword.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Assert, RuleLanguageNouns.AllOf)]
    internal sealed class AssertAllOfCommand : RuleKeyword
    {
        [Parameter(Mandatory = true, Position = 0)]
        public ScriptBlock Body { get; set; }

        protected override void ProcessRecord()
        {
            if (!IsConditionScope())
                throw new RuleRuntimeException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.KeywordConditionScope, LanguageKeywords.AllOf));

            var invokeResult = RuleConditionResult.Create(Body.Invoke());
            var result = invokeResult.AllOf();

            RunspaceContext.CurrentThread.VerboseConditionResult(condition: RuleLanguageNouns.AllOf, pass: invokeResult.Pass, count: invokeResult.Count, outcome: result);
            WriteObject(result);
        }
    }
}
