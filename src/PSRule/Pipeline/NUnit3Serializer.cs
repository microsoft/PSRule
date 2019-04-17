using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PSRule.Rules;

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
            // <?xml version="1.0" encoding="utf-8" standalone="no"?>
            // <test-results xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="nunit_schema_2.5.xsd" name="Pester" total="128" errors="0" failures="0" not-run="0" inconclusive="0" ignored="0" skipped="0" invalid="0" date="2019-04-10" time="08:27:20">
            // <environment user="bewhite" machine-name="BEBOOK" cwd="C:\Dev\Workspace\PSRule" user-domain="SOUTHPACIFIC" platform="Microsoft Windows 10 Enterprise|C:\WINDOWS|\Device\Harddisk0\Partition3" nunit-version="2.5.8.0" os-version="10.0.17763" clr-version="4.0.30319.42000" />
            // <culture-info current-culture="en-AU" current-uiculture="en-US" />

            _Builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>");

            var time = o.Sum(r => r.Time);

            _Builder.Append($"<test-results xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"nunit_schema_2.5.xsd\" name=\"PSRule\" total=\"128\" errors=\"0\" failures=\"0\" not-run=\"0\" inconclusive=\"0\" ignored=\"0\" skipped=\"0\" invalid=\"0\" date=\"2019-04-10\" time=\"{TimeSpan.FromMilliseconds(time * 1000)}\">");
            _Builder.Append($"<culture-info current-culture=\"{System.Threading.Thread.CurrentThread.CurrentCulture.ToString()}\" current-uiculture=\"{System.Threading.Thread.CurrentThread.CurrentUICulture.ToString()}\" />");

            foreach (var result in o)
            {
                var records = result.AsRecord()
                    .Select(r => new TestCase(name: r.RuleName, description: r.Message, success: r.IsSuccess(), executed: r.IsProcessed(), time: r.Time))
                    .ToArray();
                var fixture = new TestFixture(name: result.TargetName, description: "", success: result.IsSuccess(), executed: result.IsProcessed(), time: result.Time, testCases: records);

                VisitFixture(fixture: fixture);
            }

            _Builder.Append("</test-results>");

            return _Builder.ToString();
        }

        private void VisitFixture(TestFixture fixture)
        {
            _Builder.Append($"<test-suite type=\"TestFixture\" name=\"{fixture.Name}\" executed=\"{fixture.Executed}\" result=\"Success\" success=\"{fixture.Success}\" time=\"{fixture.Time}\" asserts=\"0\" description=\"{fixture.Description}\"><results>");

            foreach (var testCase in fixture.Results)
            {
                VisitTestCase(testCase: testCase);
            }

            _Builder.Append("</results></test-suite>");
        }

        private void VisitTestCase(TestCase testCase)
        {
            _Builder.Append($"<test-case description=\"{testCase.Description}\" name=\"{testCase.Name}\" time=\"{testCase.Time}\" asserts=\"0\" success=\"{testCase.Success}\" result=\"Success\" executed=\"True\" />");
        }

        private sealed class TestFixture
        {
            public readonly string Name;
            public readonly string Description;
            public readonly bool Success;
            public readonly bool Executed;
            public readonly float Time;
            public readonly TestCase[] Results;

            public TestFixture(string name, string description, bool success, bool executed, float time, TestCase[] testCases)
            {
                Name = name;
                Description = description;
                Success = success;
                Executed = executed;
                Time = time;
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
