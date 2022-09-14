// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Definitions.Rules;
using PSRule.Pipeline.Formatters;
using PSRule.Resources;
using PSRule.Rules;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A helper to construct the pipeline for Assert-PSRule.
    /// </summary>
    internal sealed class AssertPipelineBuilder : InvokePipelineBuilderBase
    {
        private AssertWriter _Writer;

        internal AssertPipelineBuilder(Source[] source, IHostContext hostContext)
            : base(source, hostContext) { }

        /// <summary>
        /// A writer for outputting assertions.
        /// </summary>
        private sealed class AssertWriter : PipelineWriter
        {
            internal readonly IAssertFormatter _Formatter;
            private readonly PipelineWriter _InnerWriter;
            private readonly string _ResultVariableName;
            private readonly IHostContext _HostContext;
            private readonly List<RuleRecord> _Results;
            private int _ErrorCount;
            private int _FailCount;
            private int _TotalCount;
            private bool _PSError;
            private SeverityLevel _Level;

            internal AssertWriter(PSRuleOption option, Source[] source, PipelineWriter inner, PipelineWriter next, OutputStyle style, string resultVariableName, IHostContext hostContext)
                : base(inner, option)
            {
                _InnerWriter = next;
                _ResultVariableName = resultVariableName;
                _HostContext = hostContext;
                if (!string.IsNullOrEmpty(resultVariableName))
                    _Results = new List<RuleRecord>();

                _Formatter = GetFormatter(GetStyle(style), source, inner, option);
            }

            private static IAssertFormatter GetFormatter(OutputStyle style, Source[] source, PipelineWriter inner, PSRuleOption option)
            {
                if (style == OutputStyle.AzurePipelines)
                    return new AzurePipelinesFormatter(source, inner, option);

                if (style == OutputStyle.GitHubActions)
                    return new GitHubActionsFormatter(source, inner, option);

                if (style == OutputStyle.VisualStudioCode)
                    return new VisualStudioCodeFormatter(source, inner, option);

                return style == OutputStyle.Plain ?
                    (IAssertFormatter)new PlainFormatter(source, inner, option) :
                    new ClientFormatter(source, inner, option);
            }

            private static OutputStyle GetStyle(OutputStyle style)
            {
                if (style != OutputStyle.Detect)
                    return style;

                if (EnvironmentHelper.Default.IsAzurePipelines())
                    return OutputStyle.AzurePipelines;

                if (EnvironmentHelper.Default.IsGitHubActions())
                    return OutputStyle.GitHubActions;

                return EnvironmentHelper.Default.IsVisualStudioCode() ?
                    OutputStyle.VisualStudioCode :
                    OutputStyle.Client;
            }

            public override void WriteObject(object sendToPipeline, bool enumerateCollection)
            {
                if (!(sendToPipeline is InvokeResult result))
                    return;

                ProcessResult(result);
                if (_InnerWriter != null)
                    _InnerWriter.WriteObject(sendToPipeline, enumerateCollection);
            }

            public override void WriteWarning(string message)
            {
                var warningPreference = _HostContext.GetPreferenceVariable(WarningPreference);
                if (warningPreference == ActionPreference.Ignore || warningPreference == ActionPreference.SilentlyContinue)
                    return;

                _Formatter.Warning(new WarningRecord(message));
            }

            public override void WriteError(ErrorRecord errorRecord)
            {
                var errorPreference = _HostContext.GetPreferenceVariable(ErrorPreference);
                if (errorPreference == ActionPreference.Ignore || errorPreference == ActionPreference.SilentlyContinue)
                    return;

                _PSError = true;
                _Formatter.Error(errorRecord);
            }

            public override void Begin()
            {
                base.Begin();
                _Formatter.Begin();
            }

            public override void End()
            {
                _Formatter.End(_TotalCount, _FailCount, _ErrorCount);
                base.End();
                try
                {
                    if (_ErrorCount > 0)
                    {
                        base.WriteError(new ErrorRecord(
                            new FailPipelineException(PSRuleResources.RuleErrorPipelineException),
                            "PSRule.Error",
                            ErrorCategory.InvalidOperation,
                            null));
                    }
                    else if (_FailCount > 0 && _Level == SeverityLevel.Error)
                    {
                        base.WriteError(new ErrorRecord(
                            new FailPipelineException(PSRuleResources.RuleFailPipelineException),
                            "PSRule.Fail",
                            ErrorCategory.InvalidData,
                            null));
                    }
                    else if (_PSError)
                    {
                        base.WriteError(new ErrorRecord(
                            new FailPipelineException(PSRuleResources.ErrorPipelineException),
                            "PSRule.Error",
                            ErrorCategory.InvalidOperation,
                            null));
                    }
                    if (_FailCount > 0 && _Level == SeverityLevel.Warning)
                        base.WriteWarning(PSRuleResources.RuleFailPipelineException);

                    if (_FailCount > 0 && _Level == SeverityLevel.Information)
                        base.WriteHost(new HostInformationMessage() { Message = PSRuleResources.RuleFailPipelineException });

                    if (_Results != null && _HostContext != null)
                        _HostContext.SetVariable(_ResultVariableName, _Results.ToArray());
                }
                finally
                {
                    if (_InnerWriter != null)
                        _InnerWriter.End();
                }
            }

            private void ProcessResult(InvokeResult result)
            {
                _Formatter.Result(result);
                _FailCount += result.Fail;
                _ErrorCount += result.Error;
                _TotalCount += result.Total;
                _Level = _Level.GetWorstCase(result.Level);
                if (_Results != null)
                    _Results.AddRange(result.AsRecord());
            }
        }

        protected override PipelineWriter PrepareWriter()
        {
            return GetWriter();
        }

        protected override PipelineWriter GetOutput(bool writeHost = false)
        {
            return base.GetOutput(writeHost: true);
        }

        private AssertWriter GetWriter()
        {
            if (_Writer == null)
            {
                var next = ShouldOutput() ? base.PrepareWriter() : null;
                _Writer = new AssertWriter(
                    option: Option,
                    source: Source,
                    inner: GetOutput(writeHost: true),
                    next: next,
                    style: Option.Output.Style ?? OutputOption.Default.Style.Value,
                    resultVariableName: _ResultVariableName,
                    hostContext: HostContext
                );
            }
            return _Writer;
        }

        private bool ShouldOutput()
        {
            return !(string.IsNullOrEmpty(Option.Output.Path) ||
                Option.Output.Format == OutputFormat.Wide ||
                Option.Output.Format == OutputFormat.None);
        }

        public sealed override IPipeline Build(IPipelineWriter writer = null)
        {
            writer ??= PrepareWriter();
            Unblock(writer);
            return !RequireModules() || !RequireSources()
                ? null
                : (IPipeline)new InvokeRulePipeline(
                    context: PrepareContext(
                        bindTargetName: BindTargetNameHook,
                        bindTargetType: BindTargetTypeHook,
                        bindField: BindFieldHook),
                    source: Source,
                    writer: writer ?? PrepareWriter(),
                    outcome: RuleOutcome.Processed);
        }
    }
}
