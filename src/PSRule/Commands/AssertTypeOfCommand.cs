// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using PSRule.Resources;
using System.Linq;
using System.Management.Automation;
using System.Threading;

namespace PSRule.Commands
{
    /// <summary>
    /// The TypeOf keyword.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Assert, RuleLanguageNouns.TypeOf)]
    internal sealed class AssertTypeOfCommand : RuleKeyword
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string[] TypeName { get; set; }

        [Parameter(Mandatory = false)]
        public string Reason { get; set; }

        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        protected override void ProcessRecord()
        {
            if (!IsRuleScope())
                throw RuleScopeException(LanguageKeywords.TypeOf);

            var inputObject = InputObject ?? GetTargetObject();
            var result = false;

            if (inputObject != null)
            {
                var actualTypeNames = PSObject.AsPSObject(inputObject).TypeNames.ToArray();
                result = (actualTypeNames.Intersect(TypeName).Any());
            }

            RunspaceContext.CurrentThread.VerboseConditionResult(condition: RuleLanguageNouns.TypeOf, outcome: result);
            if (!(result || TryReason(Reason)))
            {
                WriteReason(string.Format(Thread.CurrentThread.CurrentCulture, ReasonStrings.TypeOf, string.Join(", ", TypeName)));
            }
            WriteObject(result);
        }
    }
}
