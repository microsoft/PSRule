using PSRule.Parser;
using System;
using System.IO;
using Xunit;

namespace PSRule
{
    public sealed class RuleDocumentTests
    {
        [Fact]
        public void ReadDocument()
        {
            var document = GetDocument();

            Assert.Equal("Kubernetes.Deployment.NotLatestImage", document.Name);
            Assert.Equal("Containers should use specific tags instead of latest.", document.Synopsis.Text);
            Assert.Single(document.Recommendation);
            Assert.Equal(@"Deployments or pods should identify a specific tag to use for container images instead of latest. When latest is used it may be hard to determine which version of the image is running.
When using variable tags such as v1.0 (which may refer to v1.0.0 or v1.0.1) consider using imagePullPolicy: Always to ensure that the an out-of-date cached image is not used.
The latest tag automatically uses imagePullPolicy: Always instead of the default imagePullPolicy: IfNotPresent."
            , document.Recommendation[0].Introduction);
            Assert.Equal("Critical", document.Annotations["severity"]);
            Assert.Equal("Pod security", document.Annotations["category"]);
        }

        private RuleDocument GetDocument()
        {
            var tokens = GetToken();
            var lexer = new RuleLexer(preserveFomatting: false);
            return lexer.Process(stream: tokens);
        }

        private TokenStream GetToken()
        {
            var reader = new MarkdownReader(yamlHeaderOnly: false);
            return reader.Read(GetMarkdownContent(), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RuleDocument.md"));
        }

        private string GetMarkdownContent()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RuleDocument.md");
            return File.ReadAllText(path);
        }
    }
}
