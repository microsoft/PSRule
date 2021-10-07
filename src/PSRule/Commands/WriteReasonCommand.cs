// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Runtime;

namespace PSRule.Commands
{
    /// <summary>
    /// The Reason keyword.
    /// </summary>
    [Cmdlet(VerbsCommunications.Write, RuleLanguageNouns.Reason)]
    internal sealed class WriteReasonCommand : RuleKeyword
    {
        [Parameter(Mandatory = false, Position = 0)]
        public string Text { get; set; }

        protected override void ProcessRecord()
        {
            if (!IsConditionScope())
                throw ConditionScopeException(LanguageKeywords.Reason);


            if (MyInvocation.BoundParameters.ContainsKey(nameof(Text)))
                RunspaceContext.CurrentThread.WriteReason(text: Text);
        }
    }
}
