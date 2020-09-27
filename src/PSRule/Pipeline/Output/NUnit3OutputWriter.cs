// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Common;
using PSRule.Configuration;
using PSRule.Resources;
using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace PSRule.Pipeline.Output
{
    internal sealed class NUnit3OutputWriter : SerializationOutputWriter<InvokeResult>
    {
        private readonly StringBuilder _Builder;

        internal NUnit3OutputWriter(PipelineWriter inner, PSRuleOption option)
            : base(inner, option)
        {
            _Builder = new StringBuilder();
        }

        public override void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (!(sendToPipeline is InvokeResult result))
                return;

            Add(result);
        }

        protected override string Serialize(InvokeResult[] o)
        {
            _Builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>");

            float time = o.Sum(r => r.Time);
            var total = o.Sum(r => r.Total);
            var error = o.Sum(r => r.Error);
            var fail = o.Sum(r => r.Fail);

            _Builder.Append($"<test-results xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"nunit_schema_2.5.xsd\" name=\"PSRule\" total=\"{total}\" errors=\"{error}\" failures=\"{fail}\" not-run=\"0\" inconclusive=\"0\" ignored=\"0\" skipped=\"0\" invalid=\"0\" date=\"{DateTime.UtcNow.ToString("yyyy-MM-dd", Thread.CurrentThread.CurrentCulture)}\" time=\"{TimeSpan.FromMilliseconds(time)}\">");
            _Builder.Append($"<environment user=\"{Environment.UserName}\" machine-name=\"{Environment.MachineName}\" cwd=\"{Configuration.PSRuleOption.GetWorkingPath()}\" user-domain=\"{Environment.UserDomainName}\" platform=\"{Environment.OSVersion.Platform}\" nunit-version=\"2.5.8.0\" os-version=\"{Environment.OSVersion.Version}\" clr-version=\"{Environment.Version}\" />");
            _Builder.Append($"<culture-info current-culture=\"{Thread.CurrentThread.CurrentCulture}\" current-uiculture=\"{Thread.CurrentThread.CurrentUICulture}\" />");
            foreach (var result in o)
            {
                if (result.Total == 0)
                    continue;

                var records = result.AsRecord();
                var testCases = records
                    .Select(r => new TestCase(
                        name: string.Concat(r.TargetName, " -- ", r.RuleName),
                        description: r.Info.Synopsis,
                        success: r.IsSuccess(),
                        executed: r.IsProcessed(),
                        time: r.Time,
                        failureMessage: FailureMessage(r),
                        scriptStackTrace: r.Error?.ScriptStackTrace
                    ))
                    .ToArray();
                var failedCount = testCases.Count(r => !r.Success);
                var fixture = new TestFixture(
                    name: records[0].TargetName,
                    description: "",
                    success: result.IsSuccess(),
                    executed: result.IsProcessed(),
                    time: result.Time,
                    asserts: failedCount,
                    testCases: testCases
                );
                VisitFixture(fixture: fixture);
            }
            _Builder.Append("</test-results>");
            return _Builder.ToString();
        }

        private void VisitFixture(TestFixture fixture)
        {
            _Builder.Append($"<test-suite type=\"TestFixture\" name=\"{fixture.Name}\" executed=\"{fixture.Executed}\" result=\"{(fixture.Success ? "Success" : "Failure")}\" success=\"{fixture.Success}\" time=\"{fixture.Time.ToString(Thread.CurrentThread.CurrentCulture)}\" asserts=\"{fixture.Asserts}\" description=\"{fixture.Description}\"><results>");
            foreach (var testCase in fixture.Results)
                VisitTestCase(testCase: testCase);

            _Builder.Append("</results></test-suite>");
        }

        private void VisitTestCase(TestCase testCase)
        {
            _Builder.Append($"<test-case description=\"{testCase.Description}\" name=\"{testCase.Name}\" time=\"{testCase.Time.ToString(Thread.CurrentThread.CurrentCulture)}\" asserts=\"0\" success=\"{testCase.Success}\" result=\"{(testCase.Success ? "Success" : "Failure")}\" executed=\"{testCase.Executed}\">");
            if (!testCase.Success)
            {
                _Builder.Append("<failure>");
                _Builder.Append($"<message><![CDATA[{testCase.FailureMessage}]]></message>");
                _Builder.Append($"<stack-trace><![CDATA[{testCase.ScriptStackTrace}]]></stack-trace>");
                _Builder.Append("</failure>");
            }
            _Builder.Append("</test-case>");
        }

        private string FailureMessage(Rules.RuleRecord record)
        {
            var useMarkdown = Option.Output.Style == OutputStyle.AzurePipelines;
            var sb = new StringBuilder();
            sb.AppendLine(record.Recommendation);
            var link = record.Info.GetOnlineHelpUri();
            if (useMarkdown && link != null)
                sb.AppendLine(string.Format(Thread.CurrentThread.CurrentCulture, ReportStrings.NUnit_DetailsLink, link));

            if (record.Reason != null && record.Reason.Length > 0)
            {
                sb.AppendLine();
                sb.AppendLine(ReportStrings.NUnit_ReportedReasons);
                sb.AppendLine();
                for (var i = 0; i < record.Reason.Length; i++)
                {
                    sb.Append("- ");
                    if (useMarkdown)
                    {
                        sb.AppendMarkdownText(record.Reason[i]);
                        sb.AppendLine();
                    }
                    else
                        sb.AppendLine(record.Reason[i]);
                }
            }
            return sb.ToString();
        }

        private sealed class TestFixture
        {
            public readonly string Name;
            public readonly string Description;
            public readonly bool Success;
            public readonly bool Executed;
            public readonly float Time;
            public readonly int Asserts;
            public readonly TestCase[] Results;

            public TestFixture(string name, string description, bool success, bool executed, long time, int asserts, TestCase[] testCases)
            {
                Name = name;
                Description = description;
                Success = success;
                Executed = executed;
                Time = time / 1000f;
                Asserts = asserts;
                Results = testCases;
            }
        }

        private sealed class TestCase
        {
            public readonly string Name;
            public readonly string Description;
            public readonly bool Success;
            public readonly bool Executed;
            public readonly float Time;
            public readonly string FailureMessage;
            public readonly string ScriptStackTrace;

            public TestCase(string name, string description, bool success, bool executed, long time, string failureMessage, string scriptStackTrace)
            {
                Name = name;
                Description = description;
                Success = success;
                Executed = executed;
                Time = time / 1000f;
                FailureMessage = failureMessage;
                ScriptStackTrace = scriptStackTrace;
            }
        }
    }
}
