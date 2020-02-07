// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Rules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Reflection;

namespace PSRule.Pipeline
{
    internal interface IAssertFormatter : ILogger
    {
        void Result(InvokeResult result);

        void Error(ErrorRecord errorRecord);

        void Warning(WarningRecord warningRecord);

        void End(int total, int fail, int error);
    }

    /// <summary>
    /// A helper to construct the pipeline for Assert-PSRule.
    /// </summary>
    internal sealed class AssertPipelineBuilder : InvokePipelineBuilderBase
    {
        private AssertWriter _Writer;

        internal AssertPipelineBuilder(Source[] source)
            : base(source) { }

        /// <summary>
        /// A writer for outputting assertions.
        /// </summary>
        private sealed class AssertWriter : PipelineWriter
        {
            internal readonly IAssertFormatter _Formatter;
            private readonly PipelineWriter _InnerWriter;
            private int _ErrorCount = 0;
            private int _FailCount = 0;
            private int _TotalCount = 0;

            internal AssertWriter(PSRuleOption option, Source[] source, PipelineWriter inner, PipelineWriter next, OutputStyle style)
                : base(inner, option)
            {
                _InnerWriter = next;
                if (style == OutputStyle.AzurePipelines)
                    _Formatter = new AzurePipelinesFormatter(source, inner);
                else if (style == OutputStyle.GitHubActions)
                    _Formatter = new GitHubActionsFormatter(source, inner);
                else if (style == OutputStyle.Plain)
                    _Formatter = new PlainFormatter(source, inner);
                else if (style == OutputStyle.Client)
                    _Formatter = new ClientFormatter(source, inner);
            }

            /// <summary>
            /// A base class for a formatter.
            /// </summary>
            private abstract class AssertFormatterBase : PipelineLoggerBase, IAssertFormatter
            {
                protected readonly ILogger Logger;

                protected AssertFormatterBase(Source[] source, ILogger logger)
                {
                    Logger = logger;
                    Banner();
                    Source(source);
                }

                public void Error(ErrorRecord errorRecord)
                {
                    Error(errorRecord.Exception.Message);
                }

                public void Warning(WarningRecord warningRecord)
                {
                    Warning(warningRecord.Message);
                }

                public abstract void Result(InvokeResult result);

                protected abstract void Error(string message);

                protected abstract void Warning(string message);

                protected void Banner()
                {
                    Write(FormatterStrings.Banner.Replace("\\n", Environment.NewLine));
                    Write();
                }

                protected string StartObject(RuleRecord record)
                {
                    return string.Concat(" -> ", record.TargetName, " : ", record.TargetType);
                }

                private void Source(Source[] source)
                {
                    var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
                    Write(string.Format(FormatterStrings.PSRuleVersion, version));

                    var list = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    for (var i = 0; source != null && i < source.Length; i++)
                    {
                        if (source[i].Module != null && !list.Contains(source[i].Module.Name))
                        {
                            Write(string.Format(FormatterStrings.ModuleVersion, source[i].Module.Name, source[i].Module.Version));
                            list.Add(source[i].Module.Name);
                        }
                    }
                    Write();
                }

                protected override void DoWriteError(ErrorRecord errorRecord)
                {
                    Error(errorRecord);
                }

                protected override void DoWriteWarning(string message)
                {
                    Warning(message);
                }

                protected override void DoWriteVerbose(string message)
                {
                    Logger.WriteVerbose(message);
                }

                protected override void DoWriteInformation(InformationRecord informationRecord)
                {
                    Logger.WriteInformation(informationRecord);
                }

                protected override void DoWriteDebug(DebugRecord debugRecord)
                {
                    Logger.WriteDebug(debugRecord);
                }

                protected override void DoWriteObject(object sendToPipeline, bool enumerateCollection)
                {
                    Logger.WriteObject(sendToPipeline, enumerateCollection);
                }

                public void End(int total, int fail, int error)
                {
                    Write();
                    Write(string.Format(FormatterStrings.Summary, total, fail, error));
                }

                protected void Write(string message = "")
                {
                    Logger.WriteHost(new HostInformationMessage { Message = message });
                }
            }

            /// <summary>
            /// Client assert formatter.
            /// </summary>
            private sealed class ClientFormatter : AssertFormatterBase, IAssertFormatter
            {
                internal ClientFormatter(Source[] source, ILogger logger)
                    : base(source, logger) { }

                public override void Result(InvokeResult result)
                {
                    var records = result.AsRecord();
                    for (var i = 0; i < records.Length; i++)
                    {
                        if (i == 0)
                        {
                            Empty();
                            Green(StartObject(records[i]));
                            Empty();
                        }

                        if (records[i].IsSuccess())
                            Green(string.Format(FormatterStrings.Client_Pass, records[i].RuleName));
                        else if (records[i].Outcome == RuleOutcome.Error)
                            Red(string.Format(FormatterStrings.Client_Error, records[i].RuleName));
                        else
                            Red(string.Format(FormatterStrings.Client_Fail, records[i].RuleName));
                    }
                }

                protected override void Error(string message)
                {
                    Red(string.Format(FormatterStrings.Client_Error, message));
                }

                protected override void Warning(string message)
                {
                    Yellow(string.Format(FormatterStrings.Client_Warning, message));
                }

                private void Empty()
                {
                    Logger.WriteHost(new HostInformationMessage() { Message = string.Empty });
                }

                private void Green(string message)
                {
                    Logger.WriteHost(new HostInformationMessage() { Message = message, ForegroundColor = ConsoleColor.Green });
                }

                private void Red(string message)
                {
                    Logger.WriteHost(new HostInformationMessage() { Message = message, ForegroundColor = ConsoleColor.Red });
                }

                private void Yellow(string message)
                {
                    Logger.WriteHost(new HostInformationMessage() { Message = message, ForegroundColor = ConsoleColor.Yellow });
                }
            }

            /// <summary>
            /// Plain text assert formatter.
            /// </summary>
            private sealed class PlainFormatter : AssertFormatterBase, IAssertFormatter
            {
                internal PlainFormatter(Source[] source, ILogger logger)
                    : base(source, logger) { }

                public override void Result(InvokeResult result)
                {
                    var records = result.AsRecord();
                    for (var i = 0; i < records.Length; i++)
                    {
                        if (i == 0)
                        {
                            Write();
                            Write(StartObject(records[i]));
                            Write();
                        }

                        if (records[i].IsSuccess())
                            Write(string.Format(FormatterStrings.Plain_Pass, records[i].RuleName));
                        else
                            Write(string.Format(FormatterStrings.Plain_Fail, records[i].RuleName));
                    }
                }

                protected override void DoWriteError(ErrorRecord errorRecord)
                {
                    Error(errorRecord);
                }

                protected override void DoWriteWarning(string message)
                {
                    Warning(message);
                }

                protected override void Error(string message)
                {
                    Write(string.Format(FormatterStrings.Plain_Error, message));
                }

                protected override void Warning(string message)
                {
                    Write(string.Format(FormatterStrings.Plain_Warning, message));
                }
            }

            /// <summary>
            /// Formatter for Azure Pipelines.
            /// </summary>
            private sealed class AzurePipelinesFormatter : AssertFormatterBase, IAssertFormatter
            {
                private bool _WasInfo = false;

                internal AzurePipelinesFormatter(Source[] source, ILogger logger)
                    : base(source, logger) { }

                public override void Result(InvokeResult result)
                {
                    var records = result.AsRecord();
                    for (var i = 0; i < records.Length; i++)
                    {
                        if (i == 0)
                        {
                            Write();
                            Write(StartObject(records[i]));
                            _WasInfo = true;
                        }
                        if (_WasInfo)
                            Write();
                        _WasInfo = false;

                        if (records[i].IsSuccess())
                            Write(string.Concat("    [+] ", records[i].RuleName));
                        else if (records[i].Outcome == RuleOutcome.Error)
                        {
                            Write(string.Concat("    [-] ", records[i].RuleName));
                            GetError(records[i]);
                        }
                        else
                        {
                            Write(string.Concat("    [-] ", records[i].RuleName));
                            Error(string.Format(FormatterStrings.AzurePipelines_Fail, records[i].TargetName, records[i].RuleName, GetReason(records[i])));
                        }
                    }
                }

                private string GetReason(RuleRecord record)
                {
                    return string.Join(" ", record.Reason);
                }

                private void GetError(RuleRecord record)
                {
                    if (record.Error == null)
                        return;

                    Error(string.Format(FormatterStrings.AzurePipelines_Error, record.TargetName, record.RuleName, record.Error.Message));
                    Write();
                    Write(record.Error.ScriptStackTrace);
                }

                protected override void Error(string message)
                {
                    if (!_WasInfo)
                        Write();

                    Write(string.Concat("##vso[task.logissue type=error]", message));
                    _WasInfo = true;
                }

                protected override void Warning(string message)
                {
                    if (!_WasInfo)
                        Write();

                    Write(string.Concat("##vso[task.logissue type=warning]", message));
                    _WasInfo = true;
                }
            }

            /// <summary>
            /// Formatter for GitHub Actions.
            /// </summary>
            private sealed class GitHubActionsFormatter : AssertFormatterBase, IAssertFormatter
            {
                private bool _WasInfo = false;

                internal GitHubActionsFormatter(Source[] source, ILogger logger)
                    : base(source, logger) { }

                public override void Result(InvokeResult result)
                {
                    var records = result.AsRecord();
                    for (var i = 0; i < records.Length; i++)
                    {
                        if (i == 0)
                        {
                            Write();
                            Write(StartObject(records[i]));
                            _WasInfo = true;
                        }
                        if (_WasInfo)
                            Write();
                        _WasInfo = false;

                        if (records[i].IsSuccess())
                            Write(string.Concat("    [+] ", records[i].RuleName));
                        else
                        {
                            Write(string.Concat("    [-] ", records[i].RuleName));
                            Error(string.Format(FormatterStrings.GitHubActions_Fail, records[i].TargetName, records[i].RuleName, GetReason(records[i])));
                        }
                    }
                }

                private string GetReason(RuleRecord record)
                {
                    return string.Join(" ", record.Reason);
                }

                protected override void Error(string message)
                {
                    if (!_WasInfo)
                        Write();

                    Write(string.Concat("::error::", message));
                    _WasInfo = true;
                }

                protected override void Warning(string message)
                {
                    if (!_WasInfo)
                        Write();

                    Write(string.Concat("::warning::", message));
                    _WasInfo = true;
                }
            }

            public override void WriteObject(object sendToPipeline, bool enumerateCollection)
            {
                if (!(sendToPipeline is InvokeResult result))
                    return;

                _Formatter.Result(result);
                _FailCount += result.Fail;
                _ErrorCount += result.Error;
                _TotalCount += result.Total;

                if (_InnerWriter != null)
                    _InnerWriter.WriteObject(sendToPipeline, enumerateCollection);
            }

            public override void WriteWarning(string message)
            {
                _Formatter.Warning(new WarningRecord(message));
            }

            public override void End()
            {
                _Formatter.End(_TotalCount, _FailCount, _ErrorCount);
                base.End();
                try
                {
                    if (_ErrorCount > 0)
                        base.WriteError(new ErrorRecord(new FailPipelineException(PSRuleResources.ErrorPipelineException), "PSRule.Error", ErrorCategory.InvalidOperation, null));
                    else if (_FailCount > 0)
                        base.WriteError(new ErrorRecord(new FailPipelineException(PSRuleResources.FailPipelineException), "PSRule.Fail", ErrorCategory.InvalidData, null));
                }
                finally
                {
                    if (_InnerWriter != null)
                        _InnerWriter.End();
                }
            }
        }

        protected override PipelineWriter PrepareWriter()
        {
            return GetWriter();
        }

        private AssertWriter GetWriter()
        {
            if (_Writer == null)
            {
                var next = ShouldOutput() ? base.PrepareWriter() : null;
                _Writer = new AssertWriter(Option, Source, GetOutput(), next, Option.Output.Style ?? OutputOption.Default.Style.Value);
            }
            return _Writer;
        }

        private bool ShouldOutput()
        {
            return !(string.IsNullOrEmpty(Option.Output.Path) ||
                Option.Output.Format == OutputFormat.Wide ||
                Option.Output.Format == OutputFormat.None);
        }
    }
}
