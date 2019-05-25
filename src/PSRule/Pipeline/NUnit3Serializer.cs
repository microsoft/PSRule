﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSRule.Pipeline
{
    internal sealed class NUnit3Serializer
    {
        private readonly StringBuilder _Builder;

        public NUnit3Serializer()
        {
            _Builder = new StringBuilder();
        }

        internal string Serialize(IEnumerable<InvokeResult> o)
        {
            _Builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>");

            var time = o.Sum(r => r.Time);
            var total = o.Sum(r => r.Total);
            var error = o.Sum(r => r.Error);
            var fail = o.Sum(r => r.Fail);

            _Builder.Append($"<test-results xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"nunit_schema_2.5.xsd\" name=\"PSRule\" total=\"{total}\" errors=\"{error}\" failures=\"{fail}\" not-run=\"0\" inconclusive=\"0\" ignored=\"0\" skipped=\"0\" invalid=\"0\" date=\"{DateTime.UtcNow.ToString("yyyy-MM-dd")}\" time=\"{TimeSpan.FromMilliseconds(time * 1000)}\">");
            _Builder.Append($"<environment user=\"{Environment.UserName}\" machine-name=\"{Environment.MachineName}\" cwd=\"{Configuration.PSRuleOption.GetWorkingPath()}\" user-domain=\"{Environment.UserDomainName}\" platform=\"{Environment.OSVersion.Platform}\" nunit-version=\"2.5.8.0\" os-version=\"{Environment.OSVersion.Version}\" clr-version=\"{Environment.Version.ToString()}\" />");
            _Builder.Append($"<culture-info current-culture=\"{System.Threading.Thread.CurrentThread.CurrentCulture.ToString()}\" current-uiculture=\"{System.Threading.Thread.CurrentThread.CurrentUICulture.ToString()}\" />");

            foreach (var result in o)
            {
                var records = result.AsRecord()
                    .Select(r => new TestCase(name: string.Concat(result.TargetName, " -- ", r.RuleName), description: r.Message, success: r.IsSuccess(), executed: r.IsProcessed(), time: r.Time))
                    .ToArray();
                var failedCount = records.Count(r => !r.Success);
                var fixture = new TestFixture(name: result.TargetName, description: "", success: result.IsSuccess(), executed: result.IsProcessed(), time: result.Time, asserts: failedCount, testCases: records);

                VisitFixture(fixture: fixture);
            }

            _Builder.Append("</test-results>");

            return _Builder.ToString();
        }

        private void VisitFixture(TestFixture fixture)
        {
            _Builder.Append($"<test-suite type=\"TestFixture\" name=\"{fixture.Name}\" executed=\"{fixture.Executed}\" result=\"{(fixture.Success ? "Success" : "Failure")}\" success=\"{fixture.Success}\" time=\"{fixture.Time}\" asserts=\"{fixture.Asserts}\" description=\"{fixture.Description}\"><results>");

            foreach (var testCase in fixture.Results)
            {
                VisitTestCase(testCase: testCase);
            }

            _Builder.Append("</results></test-suite>");
        }

        private void VisitTestCase(TestCase testCase)
        {
            _Builder.Append($"<test-case description=\"{testCase.Description}\" name=\"{testCase.Name}\" time=\"{testCase.Time}\" asserts=\"0\" success=\"{testCase.Success}\" result=\"{(testCase.Success ? "Success" : "Failure")}\" executed=\"{testCase.Executed}\" />");
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

            public TestFixture(string name, string description, bool success, bool executed, float time, int asserts, TestCase[] testCases)
            {
                Name = name;
                Description = description;
                Success = success;
                Executed = executed;
                Time = time;
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

            public TestCase(string name, string description, bool success, bool executed, float time)
            {
                Name = name;
                Description = description;
                Success = success;
                Executed = executed;
                Time = time;
            }
        }
    }
}
