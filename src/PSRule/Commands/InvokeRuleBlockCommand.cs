// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Commands
{
    /// <summary>
    /// An internal language command used to evaluate a rule script block.
    /// </summary>
    internal sealed class InvokeRuleBlockCommand : Cmdlet
    {
        [Parameter()]
        public string[] Type;

        [Parameter()]
        public ResourceId[] With;

        [Parameter()]
        public ScriptBlock If;

        [Parameter()]
        public ScriptBlock Body;

        [Parameter()]
        public SourceFile Source;

        protected override void ProcessRecord()
        {
            var context = RunspaceContext.CurrentThread;
            try
            {
                if (Body == null)
                    return;

                // Evaluate selector pre-condition
                if (!AcceptsWith())
                {
                    context.Writer.DebugMessage(PSRuleResources.DebugTargetTypeMismatch);
                    return;
                }

                // Evaluate type pre-condition
                if (!AcceptsType())
                {
                    context.Writer.DebugMessage(PSRuleResources.DebugTargetTypeMismatch);
                    return;
                }

                // Evaluate script pre-condition
                if (If != null)
                {
                    try
                    {
                        context.PushScope(RunspaceScope.Precondition);
                        context.EnterLanguageScope(Source);
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
                    context.PushScope(RunspaceScope.Rule);
                    context.EnterLanguageScope(Source);
                    var invokeResult = RuleConditionHelper.Create(Body.Invoke());
                    WriteObject(invokeResult);
                }
                finally
                {
                    context.PopScope(RunspaceScope.Rule);
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

        private bool AcceptsType()
        {
            if (Type == null)
                return true;

            var comparer = RunspaceContext.CurrentThread.LanguageScope.Binding.GetComparer();
            var targetType = RunspaceContext.CurrentThread.RuleRecord.TargetType;
            for (var i = 0; i < Type.Length; i++)
            {
                if (comparer.Equals(targetType, Type[i]))
                    return true;
            }
            return false;
        }

        private bool AcceptsWith()
        {
            if (With == null || With.Length == 0)
                return true;

            for (var i = 0; i < With.Length; i++)
            {
                if (RunspaceContext.CurrentThread.TrySelector(With[i]))
                    return true;
            }
            return false;
        }
    }
}
