// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Runtime;
using System;
using System.Management.Automation;

namespace PSRule.Commands
{
    /// <summary>
    /// An internal langauge command used to evaluate a rule script block.
    /// </summary>
    internal sealed class InvokeRuleBlockCommand : Cmdlet
    {
        [Parameter()]
        public string[] Type;

        [Parameter()]
        public ScriptBlock If;

        [Parameter()]
        public ScriptBlock Body;

        protected override void ProcessRecord()
        {
            var context = RunspaceContext.CurrentThread;
            try
            {
                if (Body == null)
                    return;

                // Evalute type pre-condition
                if (!AcceptsType())
                {
                    context.Writer.DebugMessage(PSRuleResources.DebugTargetTypeMismatch);
                    return;
                }

                // Evaluate script pre-condition
                if (If != null)
                {
                    PipelineContext.CurrentThread.ExecutionScope = ExecutionScope.Precondition;
                    var ifResult = RuleConditionHelper.Create(If.Invoke());
                    if (!ifResult.AllOf())
                    {
                        context.Writer.DebugMessage(PSRuleResources.DebugTargetIfMismatch);
                        return;
                    }
                }

                // Evaluate script block
                PipelineContext.CurrentThread.ExecutionScope = ExecutionScope.Condition;
                var invokeResult = RuleConditionHelper.Create(Body.Invoke());
                WriteObject(invokeResult);
            }
            catch (ActionPreferenceStopException ex)
            {
                context.Error(ex);
            }
            catch (RuntimeException ex)
            {
                if (ex.ErrorRecord.FullyQualifiedErrorId == "MethodInvocationNotSupportedInConstrainedLanguage")
                    throw;

                context.Error(ex);
            }
            finally
            {
                PipelineContext.CurrentThread.ExecutionScope = ExecutionScope.None;
            }
        }

        private bool AcceptsType()
        {
            if (Type == null)
                return true;

            var comparer = RunspaceContext.CurrentThread.Pipeline.Baseline.GetTargetBinding().IgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            var targetType = RunspaceContext.CurrentThread.RuleRecord.TargetType;
            for (var i = 0; i < Type.Length; i++)
            {
                if (comparer.Equals(targetType, Type[i]))
                    return true;
            }
            return false;
        }
    }
}
