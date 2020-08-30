﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Runtime;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading;

namespace PSRule.Commands
{
    /// <summary>
    /// The Exists keyword.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Assert, RuleLanguageNouns.Exists)]
    internal sealed class AssertExistsCommand : RuleKeyword
    {
        public AssertExistsCommand()
        {
            CaseSensitive = false;
            Not = false;
            All = false;
        }

        [Parameter(Mandatory = true, Position = 0)]
        public string[] Field { get; set; }

        [Parameter(Mandatory = false)]
        public string Reason { get; set; }

        [Parameter(Mandatory = false)]
        [PSDefaultValue(Value = false)]
        public SwitchParameter CaseSensitive { get; set; }

        [Parameter(Mandatory = false)]
        [PSDefaultValue(Value = false)]
        public SwitchParameter Not { get; set; }

        [Parameter(Mandatory = false)]
        [PSDefaultValue(Value = false)]
        public SwitchParameter All { get; set; }

        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        protected override void ProcessRecord()
        {
            if (!IsRuleScope())
                throw RuleScopeException(LanguageKeywords.Exists);

            var targetObject = InputObject ?? GetTargetObject();
            var foundFields = new List<string>();
            var notFoundFields = new List<string>();
            int found = 0;
            int required = All ? Field.Length : 1;

            for (var i = 0; i < Field.Length && found < required; i++)
            {
                if (ObjectHelper.GetField(bindingContext: PipelineContext.CurrentThread, targetObject: targetObject, name: Field[i], caseSensitive: CaseSensitive, value: out _))
                {
                    RunspaceContext.CurrentThread.VerboseConditionMessage(condition: RuleLanguageNouns.Exists, message: PSRuleResources.ExistsTrue, args: Field[i]);
                    foundFields.Add(Field[i]);
                    found++;
                }
                else
                    notFoundFields.Add(Field[i]);
            }

            var result = Not ? found < required : found == required;
            RunspaceContext.CurrentThread.VerboseConditionResult(condition: RuleLanguageNouns.Exists, outcome: result);
            if (!(result || TryReason(Reason)))
            {
                WriteReason(Not ? string.Format(Thread.CurrentThread.CurrentCulture, ReasonStrings.ExistsNot, string.Join(", ", foundFields)) : string.Format(Thread.CurrentThread.CurrentCulture, ReasonStrings.Exists, string.Join(", ", notFoundFields)));
            }
            WriteObject(result);
        }
    }
}
