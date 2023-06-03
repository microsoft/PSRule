// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Rules;

namespace PSRule.Pipeline.Output
{
    internal sealed class NUnit3OutputWriter : SerializationOutputWriter<InvokeResult>
    {
        internal NUnit3OutputWriter(PipelineWriter inner, PSRuleOption option, ShouldProcess shouldProcess)
            : base(inner, option, shouldProcess) { }

        public override void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (sendToPipeline is not InvokeResult result)
                return;

            Add(result);
        }

        protected override string Serialize(InvokeResult[] o)
        {
            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8, // Consider using: Option.Output.GetEncoding()
                // Consider using: Indent = true,
            };
            using var writer = new OutputStringWriter(Option);
            using var xml = XmlWriter.Create(writer, settings);
            xml.WriteStartDocument(standalone: false);

            float time = o.Sum(r => r.Time);
            var total = o.Sum(r => r.Total);
            var error = o.Sum(r => r.Error);
            var fail = o.Sum(r => r.Fail);

            xml.WriteStartElement("test-results");
            xml.WriteAttributeString("xsi", "noNamespaceSchemaLocation", "http://www.w3.org/2001/XMLSchema-instance", "nunit_schema_2.5.xsd");
            xml.WriteAttributeString("name", "PSRule");
            xml.WriteAttributeString("total", total.ToString());
            xml.WriteAttributeString("errors", error.ToString());
            xml.WriteAttributeString("failures", fail.ToString());
            xml.WriteAttributeString("not-run", "0");
            xml.WriteAttributeString("inconclusive", "0");
            xml.WriteAttributeString("ignored", "0");
            xml.WriteAttributeString("skipped", "0");
            xml.WriteAttributeString("invalid", "0");
            xml.WriteAttributeString("date", DateTime.UtcNow.ToString("yyyy-MM-dd", Thread.CurrentThread.CurrentCulture));
            xml.WriteAttributeString("time", TimeSpan.FromMilliseconds(time).ToString());

            xml.WriteStartElement("environment");
            xml.WriteAttributeString("user", System.Environment.UserName);
            xml.WriteAttributeString("machine-name", System.Environment.MachineName);
            xml.WriteAttributeString("cwd", PSRuleOption.GetWorkingPath());
            xml.WriteAttributeString("user-domain", System.Environment.UserDomainName);
            xml.WriteAttributeString("platform", System.Environment.OSVersion.Platform.ToString());
            xml.WriteAttributeString("nunit-version", "2.5.8.0");
            xml.WriteAttributeString("os-version", System.Environment.OSVersion.Version.ToString());
            xml.WriteAttributeString("clr-version", System.Environment.Version.ToString());
            xml.WriteEndElement();

            xml.WriteStartElement("culture-info");
            xml.WriteAttributeString("current-culture", Thread.CurrentThread.CurrentCulture.ToString());
            xml.WriteAttributeString("current-uiculture", Thread.CurrentThread.CurrentUICulture.ToString());
            xml.WriteEndElement();

            foreach (var result in o)
            {
                if (result.Total == 0)
                    continue;

                var records = result.AsRecord();
                var testCases = records
                    .Select(r => new TestCase(
                        name: string.Concat(r.TargetName, " -- ", r.RuleName),
                        description: r.Info.Synopsis.Text,
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
                VisitFixture(xml, fixture);
            }
            xml.WriteEndElement();
            xml.WriteEndDocument();
            xml.Flush();
            return writer.ToString();
        }

        private static void VisitFixture(XmlWriter xml, TestFixture fixture)
        {
            xml.WriteStartElement("test-suite");
            xml.WriteAttributeString("type", "TestFixture");
            xml.WriteAttributeString("name", fixture.Name);
            xml.WriteAttributeString("executed", fixture.Executed.ToString());
            xml.WriteAttributeString("result", fixture.Success ? "Success" : "Failure");
            xml.WriteAttributeString("success", fixture.Success.ToString());

            xml.WriteAttributeString("time", fixture.Time.ToString(Thread.CurrentThread.CurrentCulture));
            xml.WriteAttributeString("asserts", fixture.Asserts.ToString());
            xml.WriteAttributeString("description", fixture.Description);

            xml.WriteStartElement("results");
            foreach (var testCase in fixture.Results)
                VisitTestCase(xml, testCase);

            xml.WriteEndElement();
            xml.WriteEndElement();
        }

        private static void VisitTestCase(XmlWriter xml, TestCase testCase)
        {
            xml.WriteStartElement("test-case");
            xml.WriteAttributeString("description", testCase.Description);
            xml.WriteAttributeString("name", testCase.Name);
            xml.WriteAttributeString("time", testCase.Time.ToString(Thread.CurrentThread.CurrentCulture));
            xml.WriteAttributeString("asserts", "0");
            xml.WriteAttributeString("success", testCase.Success.ToString());
            xml.WriteAttributeString("result", testCase.Success ? "Success" : "Failure");
            xml.WriteAttributeString("executed", testCase.Executed.ToString());
            if (!testCase.Success)
            {
                xml.WriteStartElement("failure");

                xml.WriteStartElement("message");
                xml.WriteCData(testCase.FailureMessage);
                xml.WriteEndElement();

                xml.WriteStartElement("stack-trace");
                xml.WriteCData(testCase.ScriptStackTrace);
                xml.WriteEndElement();

                xml.WriteEndElement();
            }
            xml.WriteEndElement();
        }

        private string FailureMessage(RuleRecord record)
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
