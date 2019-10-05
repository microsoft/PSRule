using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Rules;
using System;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    internal interface IAssertFormatter
    {
        void Result(InvokeResult result);
    }

    internal sealed class AssertPipelineBuilder : InvokePipelineBuilderBase
    {
        internal AssertPipelineBuilder(Source[] source)
            : base(source) { }

        private sealed class AssertWriter : PipelineWriter
        {
            private readonly IAssertFormatter _Formatter;
            private readonly PipelineLogger _Logger;
            private readonly PipelineWriter _InnerWriter;
            private int _ErrorCount = 0;
            private int _FailCount = 0;
            private int _TotalCount = 0;

            internal AssertWriter(WriteOutput output, PipelineLogger logger, PipelineWriter innerWriter, OutputStyle style)
                : base(output)
            {
                _Logger = logger;
                _InnerWriter = innerWriter;
                if (style == OutputStyle.AzurePipelines)
                    _Formatter = new AzurePipelinesFormatter(logger);
                else if (style == OutputStyle.GitHubActions)
                    _Formatter = new GitHubActionsFormatter(logger);
                else if (style == OutputStyle.Plain)
                    _Formatter = new PlainFormatter(logger);
                else if (style == OutputStyle.Client)
                    _Formatter = new ClientFormatter(logger);
            }

            /// <summary>
            /// Client assert formatter.
            /// </summary>
            private sealed class ClientFormatter : IAssertFormatter
            {
                private readonly PipelineLogger _Logger;

                internal ClientFormatter(PipelineLogger logger)
                {
                    _Logger = logger;
                }

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
                            Green(string.Concat("    [PASS] ", records[i].RuleName));
                        else
                            Red(string.Concat("    [FAIL] ", records[i].RuleName));
                    }
                }

                private void Empty()
                {
                    _Logger.WriteHost(new HostInformationMessage() { Message = string.Empty });
                }

                private void Green(string message)
                {
                    _Logger.WriteHost(new HostInformationMessage() { Message = message, ForegroundColor = ConsoleColor.Green });
                }

                private void Red(string message)
                {
                    _Logger.WriteHost(new HostInformationMessage() { Message = message, ForegroundColor = ConsoleColor.Red });
                }
            }

            /// <summary>
            /// Plain text assert formatter.
            /// </summary>
            private sealed class PlainFormatter : IAssertFormatter
            {
                private readonly PipelineLogger _Logger;

                internal PlainFormatter(PipelineLogger logger)
                {
                    _Logger = logger;
                }

                public void Result(InvokeResult result)
                {
                    var records = result.AsRecord();
                    for (var i = 0; i < records.Length; i++)
                    {
                        if (i == 0)
                        {
                            _Logger.WriteObject(string.Empty, false);
                            _Logger.WriteObject(string.Concat(" -> ", records[i].TargetName, " : ", records[i].TargetType), false);
                            _Logger.WriteObject(string.Empty, false);
                        }

                        if (records[i].IsSuccess())
                            _Logger.WriteObject(string.Concat("    [PASS] ", records[i].RuleName), false);
                        else
                            _Logger.WriteObject(string.Concat("    [FAIL] ", records[i].RuleName), false);
                    }
                }
            }

            /// <summary>
            /// Formatter for Azure Pipelines.
            /// </summary>
            private sealed class AzurePipelinesFormatter : IAssertFormatter
            {
                private readonly PipelineLogger _Logger;

                internal AzurePipelinesFormatter(PipelineLogger logger)
                {
                    _Logger = logger;
                }

                public void Result(InvokeResult result)
                {
                    var records = result.AsRecord();
                    for (var i = 0; i < records.Length; i++)
                    {
                        if (i == 0)
                        {
                            _Logger.WriteObject(string.Empty, false);
                            _Logger.WriteObject(string.Concat(" -> ", records[i].TargetName, " : ", records[i].TargetType), false);
                            _Logger.WriteObject(string.Empty, false);
                        }

                        if (records[i].IsSuccess())
                            _Logger.WriteObject(string.Concat("    [+] ", records[i].RuleName), false);
                        else
                        {
                            _Logger.WriteObject(string.Concat("    [-] ", records[i].RuleName), false);
                            _Logger.WriteObject(string.Empty, false);
                            _Logger.WriteObject(string.Concat("##vso[task.logissue type=error]", records[i].TargetName, " [FAIL] ", records[i].RuleName), false);
                            Reason(records[i]);
                            if (i + 1 < records.Length)
                                _Logger.WriteObject(string.Empty, false);
                        }
                    }
                }

                private void Reason(RuleRecord record)
                {
                    foreach (var item in record.Reason)
                        _Logger.WriteObject(string.Concat("##vso[task.logissue type=error]- ", item), false);
                }
            }

            /// <summary>
            /// Formatter for GitHub Actions.
            /// </summary>
            private sealed class GitHubActionsFormatter : IAssertFormatter
            {
                private readonly PipelineLogger _Logger;

                internal GitHubActionsFormatter(PipelineLogger logger)
                {
                    _Logger = logger;
                }

                public void Result(InvokeResult result)
                {
                    var records = result.AsRecord();
                    for (var i = 0; i < records.Length; i++)
                    {
                        if (i == 0)
                        {
                            _Logger.WriteObject(string.Empty, false);
                            _Logger.WriteObject(string.Concat(" -> ", records[i].TargetName, " : ", records[i].TargetType), false);
                            _Logger.WriteObject(string.Empty, false);
                        }

                        if (records[i].IsSuccess())
                            _Logger.WriteObject(string.Concat("    [+] ", records[i].RuleName), false);
                        else
                        {
                            _Logger.WriteObject(string.Concat("    [-] ", records[i].RuleName), false);
                            _Logger.WriteObject(string.Empty, false);
                            _Logger.WriteObject(string.Concat("::error:: ", records[i].TargetName, " [FAIL] ", records[i].RuleName), false);
                            Reason(records[i]);
                            if (i+1 < records.Length)
                                _Logger.WriteObject(string.Empty, false);
                        }
                    }
                }

                private void Reason(RuleRecord record)
                {
                    foreach (var item in record.Reason)
                        _Logger.WriteObject(string.Concat("::error:: - ", item), false);
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
                _Logger.WriteObject(string.Empty, false);
                if (_FailCount > 0)
                    _Logger.WriteError(new ErrorRecord(new FailPipelineException(PSRuleResources.FailPipelineException), "PSRule.Fail", ErrorCategory.InvalidData, null));

                if (_InnerWriter != null)
                    _InnerWriter.End();
            }
        }

        protected override PipelineWriter PrepareWriter()
        {
            var innerWriter = ShouldOutput() ? base.PrepareWriter() : null;
            return new AssertWriter(GetOutput(), Logger, innerWriter, Option.Output.Style ?? OutputOption.Default.Style.Value);
        }

        private bool ShouldOutput()
        {
            return !(string.IsNullOrEmpty(Option.Output.Path) ||
                Option.Output.Format == OutputFormat.Wide ||
                Option.Output.Format == OutputFormat.None);
        }
    }
}
