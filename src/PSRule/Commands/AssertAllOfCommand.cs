﻿using PSRule.Pipeline;
using PSRule.Rules;
using System.Management.Automation;

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
            try
            {
                var invokeResult = new RuleConditionResult(Body.Invoke());

                var result = invokeResult.AllOf;

                PipelineContext.CurrentThread.WriteVerboseConditionResult(condition: "[AllOf]", pass: invokeResult.Pass, count: invokeResult.Count, outcome: result);

                WriteObject(result);
            }
            finally
            {

            }
        }
    }
}
