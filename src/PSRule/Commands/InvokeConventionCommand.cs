// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Runtime;
using System.Management.Automation;

namespace PSRule.Commands
{
    internal sealed class InvokeConventionCommand : Cmdlet
    {
        [Parameter()]
        public ScriptBlock If;

        [Parameter()]
        public ScriptBlock Body;

        [Parameter()]
        public RunspaceScope Scope;

        protected override void ProcessRecord()
        {
            var context = RunspaceContext.CurrentThread;
            try
            {
                if (Body == null)
                    return;

                // Evaluate script pre-condition
                if (If != null)
                {
                    try
                    {
                        context.PushScope(RunspaceScope.Precondition);
                        var ifResult = RuleConditionHelper.Create(If.Invoke());
                        if (!ifResult.AllOf())
                        {
                            context.Writer.DebugMessage(PSRuleResources.DebugTargetIfMismatch);
                            return;
                        }
                    }
                    finally
                    {
                        context.PopScope(RunspaceScope.Precondition);
                    }
                }

                try
                {
                    // Evaluate script block
                    context.PushScope(Scope);
                    var invokeResult = RuleConditionHelper.Create(Body.Invoke());
                    WriteObject(invokeResult);
                }
                finally
                {
                    context.PopScope(Scope);
                }
            }
            catch (ActionPreferenceStopException ex)
            {
                context.Error(ex);
            }
            catch (System.Management.Automation.RuntimeException ex)
            {
                if (ex.ErrorRecord.FullyQualifiedErrorId == "MethodInvocationNotSupportedInConstrainedLanguage")
                    throw;

                context.Error(ex);
            }
        }
    }
}
