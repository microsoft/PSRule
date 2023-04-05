// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Rules;

namespace PSRule.Pipeline.Output
{
    /// <summary>
    /// Define an pipeline writer to write a job summary to disk.
    /// </summary>
    internal sealed class JobSummaryWriter : ResultOutputWriter<InvokeResult>
    {
        private const string TICK = "✔️";
        private const string CROSS = "❌";
        private const string QUESTION = "❔";
        private const string HEADER_H1 = "# ";
        private const string HEADER_H2 = "## ";

        private readonly string _OutputPath;
        private readonly Encoding _Encoding;
        private readonly JobSummaryFormat _JobSummary;
        private readonly Source[] _Source;

        private Stream _Stream;
        private StreamWriter _Writer;
        private bool _IsDisposed;

        public JobSummaryWriter(IPipelineWriter inner, PSRuleOption option, ShouldProcess shouldProcess, string outputPath = null, Stream stream = null, Source[] source = null)
            : base(inner, option, shouldProcess)
        {
            _OutputPath = outputPath ?? PSRuleOption.GetRootedPath(Option.Output.JobSummaryPath);
            _Encoding = option.Output.GetEncoding();
            _JobSummary = JobSummaryFormat.Default;
            _Stream = stream;
            _Source = source;

            if (Option.Output.As == ResultFormat.Summary && inner != null)
                inner.WriteError(new PipelineConfigurationException("Output.As", PSRuleResources.PSR0002), "PSRule.Output.AsOutputSerialization", System.Management.Automation.ErrorCategory.InvalidOperation);
        }

        public override void Begin()
        {
            Open();
            base.Begin();
        }

        public sealed override void End()
        {
            Flush();
            base.End();
        }

        #region Helper methods

        private void Open()
        {
            if (string.IsNullOrEmpty(_OutputPath) || _IsDisposed || !CreateFile(_OutputPath))
                return;

            _Stream ??= new FileStream(_OutputPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            _Writer = new StreamWriter(_Stream, _Encoding, 2048, false);
        }

        private void Source()
        {
            if (!_JobSummary.HasFlag(JobSummaryFormat.Source) || _Source == null || _Source.Length == 0)
                return;

            H2(Summaries.JobSummary_Source);
            WriteLine(Summaries.JobSummary_IncludedModules);
            WriteLine();

            WriteLine($"{Summaries.JobSummary_Module} | {Summaries.JobSummary_Version}");
            WriteLine("------ | --------");

            var version = Engine.GetVersion();
            if (!string.IsNullOrEmpty(version))
                WriteLine($"PSRule | v{version}");

            var list = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < _Source.Length; i++)
            {
                if (_Source[i].Module != null && !list.Contains(_Source[i].Module.Name))
                {
                    var projectLink = string.IsNullOrEmpty(_Source[i].Module.ProjectUri) ? _Source[i].Module.Name : $"[{_Source[i].Module.Name}]({_Source[i].Module.ProjectUri})";
                    WriteLine($"{projectLink} | v{_Source[i].Module.Version}");
                    list.Add(_Source[i].Module.Name);
                }
            }
            WriteLine();
        }

        private void H1(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            WriteLine(string.Concat(HEADER_H1, text));
            WriteLine();
        }

        private void H2(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            WriteLine(string.Concat(HEADER_H2, text));
            WriteLine();
        }

        private void WriteLine(string text = null)
        {
            if (_Writer == null || _IsDisposed)
                return;

            _Writer.WriteLine(text ?? string.Empty);
        }

        private void WriteLine(string text, params object[] args)
        {
            if (_Writer == null || _IsDisposed)
                return;

            _Writer.WriteLine(text ?? string.Empty, args);
        }

        private void Complete()
        {
            var results = GetResults();
            H1(Summaries.JobSummary_Title);
            FinalResult(results);
            Source();
            Analysis(results);
        }

        private void Analysis(InvokeResult[] o)
        {
            if (o == null || o.Length == 0)
                return;

            H2(Summaries.JobSummary_Analysis);
            var rows = o.SelectMany(r => r.AsRecord()).Where(r => r.Outcome == RuleOutcome.Fail || r.Outcome == RuleOutcome.Error).ToArray();
            if (rows.Length > 0)
            {
                WriteLine(Summaries.JobSummary_AnalysisResults);
                WriteLine();
                WriteLine($"{Summaries.JobSummary_Name} | {Summaries.JobSummary_TargetName} | {Summaries.JobSummary_Synopsis}");
                WriteLine("---- | ----------- | --------");
            }
            else
            {
                WriteLine(Summaries.JobSummary_AnalysisEmpty);
            }
            for (var i = 0; i < rows.Length; i++)
                WriteAnalysisRow(rows[i]);
        }

        private void WriteAnalysisRow(RuleRecord record)
        {
            var link = record.Info.GetOnlineHelpUrl();
            var name = link != null ? $"[{record.RuleName}]({link})" : record.RuleName;
            WriteLine($"{name} | {record.TargetName} | {record.Info.Synopsis.Markdown}");
        }

        private void FinalResult(InvokeResult[] o)
        {
            var count = o?.Length;
            var overall = RuleOutcome.None;
            var pass = 0;
            var fail = 0;
            var total = 0;
            var error = 0;
            for (var i = 0; o != null && i < o.Length; i++)
            {
                overall = o[i].Outcome.GetWorstCase(overall);
                pass += o[i].Pass;
                fail += o[i].Fail;
                error += o[i].Error;
                total += o[i].Total;
            }
            var elapsed = PipelineContext.CurrentThread?.RunTime.Elapsed ?? TimeSpan.FromSeconds(0);
            WriteLine(Summaries.JobSummary_FinalResultMessage,
                OutcomeEmoji(overall),
                Enum.GetName(typeof(RuleOutcome), overall),
                pass + fail + error,
                count,
                elapsed
            );
            WriteLine();
        }

        private static string OutcomeEmoji(RuleOutcome outcome)
        {
            switch (outcome)
            {
                case RuleOutcome.Pass:
                    return TICK;

                case RuleOutcome.Error:
                case RuleOutcome.Fail:
                    return CROSS;

                default:
                    return QUESTION;
            }
        }

        protected override void Flush()
        {
            if (_Writer == null || _IsDisposed)
                return;

            Complete();
            _Writer.Flush();
        }

        #endregion Helper methods

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (!_IsDisposed)
            {
                if (disposing)
                {
                    _Writer?.Dispose();
                    _Stream?.Dispose();
                }
                _IsDisposed = true;
            }
            base.Dispose(disposing);
        }

        #endregion IDisposable
    }
}
