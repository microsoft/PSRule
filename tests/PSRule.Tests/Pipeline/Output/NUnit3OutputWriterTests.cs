// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using System.Xml;
using PSRule.Definitions.Rules;

namespace PSRule.Pipeline.Output;

/// <summary>
/// Tests for <see cref="NUnit3OutputWriter"/>.
/// </summary>
public sealed class NUnit3OutputWriterTests : OutputWriterBaseTests
{
    [Fact]
    public void NUnit3()
    {
        var option = GetOption();
        option.Repository.Url = "https://github.com/microsoft/PSRule.UnitTest";
        var output = new TestWriter(option);
        var result = new InvokeResult(GetRun());
        result.Add(GetPass());
        result.Add(GetFail());
        result.Add(GetFail("rid-003", SeverityLevel.Warning));
        result.Add(GetFail("rid-004", SeverityLevel.Information, synopsis: "Synopsis \"with quotes\"."));
        var writer = new NUnit3OutputWriter(output, option, null);
        writer.Begin();
        writer.WriteObject(result, false);
        writer.End(new DefaultPipelineResult(null, Options.BreakLevel.None));

        var s = output.Output.OfType<string>().FirstOrDefault();
        var doc = new XmlDocument();
        doc.LoadXml(s);

        var declaration = doc.ChildNodes.Item(0) as XmlDeclaration;
        Assert.Equal("utf-8", declaration.Encoding);
        var xml = doc["test-results"]["test-suite"].OuterXml.Replace(System.Environment.NewLine, "\r\n");
        Assert.Equal("<test-suite type=\"TestFixture\" name=\"TestObject1\" executed=\"True\" result=\"Failure\" success=\"False\" time=\"3.5\" asserts=\"3\" description=\"\"><results><test-case description=\"This is rule 001.\" name=\"TestObject1 -- rule-001\" time=\"0.5\" asserts=\"0\" success=\"True\" result=\"Success\" executed=\"True\" /><test-case description=\"This is rule 002.\" name=\"TestObject1 -- rule-002\" time=\"1\" asserts=\"0\" success=\"False\" result=\"Failure\" executed=\"True\"><failure><message><![CDATA[Recommendation for rule 002\r\n]]></message><stack-trace><![CDATA[]]></stack-trace></failure></test-case><test-case description=\"This is rule 002.\" name=\"TestObject1 -- rule-002\" time=\"1\" asserts=\"0\" success=\"False\" result=\"Failure\" executed=\"True\"><failure><message><![CDATA[Recommendation for rule 002\r\n]]></message><stack-trace><![CDATA[]]></stack-trace></failure></test-case><test-case description=\"Synopsis &quot;with quotes&quot;.\" name=\"TestObject1 -- rule-002\" time=\"1\" asserts=\"0\" success=\"False\" result=\"Failure\" executed=\"True\"><failure><message><![CDATA[Recommendation for rule 002\r\n]]></message><stack-trace><![CDATA[]]></stack-trace></failure></test-case></results></test-suite>", xml);
    }
}
