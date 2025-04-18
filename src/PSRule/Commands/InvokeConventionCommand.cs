// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Commands;

internal sealed class InvokeConventionCommand : Cmdlet
{
    [Parameter()]
    public ScriptBlock? If;

    [Parameter()]
    public ScriptBlock? Body;

    [Parameter()]
    public RunspaceScope Scope;

    protected override void ProcessRecord()
    {
        var context = LegacyRunspaceContext.CurrentThread;
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
                Body.Invoke();
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
