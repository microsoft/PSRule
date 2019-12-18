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
            private readonly ILogger _Logger;
            private readonly PipelineWriter _InnerWriter;
            private int _ErrorCount = 0;
            private int _FailCount = 0;
            private int _TotalCount = 0;

            internal AssertWriter(Source[] source, WriteOutput output, ILogger logger, PipelineWriter innerWriter, OutputStyle style)
                : base(output)
            {
                _Logger = logger;
                _InnerWriter = innerWriter;
                if (style == OutputStyle.AzurePipelines)
                    _Formatter = new AzurePipelinesFormatter(source, logger);
                else if (style == OutputStyle.GitHubActions)
                    _Formatter = new GitHubActionsFormatter(source, logger);
                else if (style == OutputStyle.Plain)
                    _Formatter = new PlainFormatter(source, logger);
                else if (style == OutputStyle.Client)
                    _Formatter = new ClientFormatter(source, logger);
            }

            /// <summary>
            /// A base class for a formatter.
            /// </summary>
            private abstract class AssertFormatterBase : PipelineLoggerBase
            {
                protected readonly ILogger Logger;

                protected AssertFormatterBase(Source[] source, ILogger logger)
                {
                    Logger = logger;
                    Banner();
                    Source(source);
                }

                protected void Banner()
                {
                    Write(FormatterStrings.Banner.Replace("\\n", Environment.NewLine));
                    Write();
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
                    Logger.WriteError(errorRecord);
                }

                protected override void DoWriteWarning(string message)
                {
                    Logger.WriteWarning(message);
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

                public void Result(InvokeResult result)
                {
                    var records = result.AsRecord();
                    for (var i = 0; i < records.Length; i++)
                    {
                        if (i == 0)
                        {
                            Empty();
                            Green(string.Concat(" -> ", records[i].TargetName, " : ", records[i].TargetType));
                            Empty();
                        }

                        if (records[i].IsSuccess())
                            Green(string.Format(FormatterStrings.Client_Pass, records[i].RuleName));
                        else
                            Red(string.Format(FormatterStrings.Client_Fail, records[i].RuleName));
                    }
                }

                public void Error(ErrorRecord errorRecord)
                {
                    Error(errorRecord.Exception.Message);
                }

                public void Warning(WarningRecord warningRecord)
                {
                    Warning(warningRecord.Message);
                }

                protected override void DoWriteError(ErrorRecord errorRecord)
                {
                    Error(errorRecord);
                }

                protected override void DoWriteWarning(string message)
                {
                    Warning(message);
                }

                private void Error(string message)
                {
                    Red(string.Format(FormatterStrings.Client_Error, message));
                }

                private void Warning(string message)
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

                public void Result(InvokeResult result)
                {
                    var records = result.AsRecord();
                    for (var i = 0; i < records.Length; i++)
                    {
                        if (i == 0)
                        {
                            Write();
                            Write(string.Concat(" -> ", records[i].TargetName, " : ", records[i].TargetType));
                            Write();
                        }

                        if (records[i].IsSuccess())
                            Write(string.Format(FormatterStrings.Plain_Pass, records[i].RuleName));
                        else
                            Write(string.Format(FormatterStrings.Plain_Fail, records[i].RuleName));
                    }
                }

                public void Error(ErrorRecord errorRecord)
                {
                    Error(errorRecord.Exception.Message);
                }

                public void Warning(WarningRecord warningRecord)
                {
                    Warning(warningRecord.Message);
                }

                protected override void DoWriteError(ErrorRecord errorRecord)
                {
                    Error(errorRecord);
                }

                protected override void DoWriteWarning(string message)
                {
                    Warning(message);
                }

                private void Error(string message)
                {
                    Write(string.Format(FormatterStrings.Plain_Error, message));
                }

                private void Warning(string message)
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

                public void Result(InvokeResult result)
                {
                    var records = result.AsRecord();
                    for (var i = 0; i < records.Length; i++)
                    {
                        if (i == 0)
                        {
                            Write();
                            Write(string.Concat(" -> ", records[i].TargetName, " : ", records[i].TargetType));
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
                            Error(string.Format(FormatterStrings.AzurePipelines_Fail, records[i].TargetName, records[i].RuleName, GetReason(records[i])));
                        }
                    }
                }

                public void Error(ErrorRecord errorRecord)
                {
                    Error(errorRecord.ErrorDetails.Message);
                }

                public void Warning(WarningRecord warningRecord)
                {
                    Warning(warningRecord.Message);
                }

                private string GetReason(RuleRecord record)
                {
                    return string.Join(" ", record.Reason);
                }

                private void Error(string message)
                {
                    if (!_WasInfo)
                        Write();

                    Write(string.Concat("##vso[task.logissue type=error]", message));
                    _WasInfo = true;
                }

                private void Warning(string message)
                {
                    if (!_WasInfo)
                        Write();

                    Write(string.Concat("##vso[task.logissue type=warning]", message));
                    _WasInfo = true;
                }

                protected override void DoWriteError(ErrorRecord errorRecord)
                {
                    Error(errorRecord);
                }

                protected override void DoWriteWarning(string message)
                {
                    Warning(message);
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

                public void Result(InvokeResult result)
                {
                    var records = result.AsRecord();
                    for (var i = 0; i < records.Length; i++)
                    {
                        if (i == 0)
                        {
                            Write();
                            Write(string.Concat(" -> ", records[i].TargetName, " : ", records[i].TargetType));
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

                public void Error(ErrorRecord errorRecord)
                {
                    Error(errorRecord.ErrorDetails.Message);
                }

                public void Warning(WarningRecord warningRecord)
                {
                    Warning(warningRecord.Message);
                }

                private string GetReason(RuleRecord record)
                {
                    return string.Join(" ", record.Reason);
                }

                private void Error(string message)
                {
                    if (!_WasInfo)
                        Write();

                    Write(string.Concat("::error::", message));
                    _WasInfo = true;
                }

                private void Warning(string message)
                {
                    if (!_WasInfo)
                        Write();

                    Write(string.Concat("::warning::", message));
                    _WasInfo = true;
                }

                protected override void DoWriteError(ErrorRecord errorRecord)
                {
                    Error(errorRecord);
                }

                protected override void DoWriteWarning(string message)
                {
                    Warning(message);
                }
            }

            public override void Write(object o, bool enumerate)
            {
                if (!(o is InvokeResult result))
                    return;

                _Formatter.Result(result);
                _FailCount += result.Fail;
                _ErrorCount += result.Error;
                _TotalCount += result.Total;

                if (_InnerWriter != null)
                    _InnerWriter.Write(o, enumerate);
            }

            public override void End()
            {
                _Formatter.End(_TotalCount, _FailCount, _ErrorCount);
                if (_FailCount > 0)
                    _Logger.WriteError(new ErrorRecord(new FailPipelineException(PSRuleResources.FailPipelineException), "PSRule.Fail", ErrorCategory.InvalidData, null));

                if (_InnerWriter != null)
                    _InnerWriter.End();
            }
        }

        protected override PipelineWriter PrepareWriter()
        {
            return GetWriter();
        }

        protected override ILogger PrepareLogger()
        {
            return GetWriter()._Formatter;
        }

        private AssertWriter GetWriter()
        {
            if (_Writer == null)
            {
                var innerWriter = ShouldOutput() ? base.PrepareWriter() : null;
                _Writer = new AssertWriter(Source, GetOutput(), Logger, innerWriter, Option.Output.Style ?? OutputOption.Default.Style.Value);
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
