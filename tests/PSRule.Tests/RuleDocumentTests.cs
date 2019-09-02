﻿using PSRule.Parser;
using PSRule.Rules;
using System;
using System.Collections;
using System.IO;
using Xunit;

namespace PSRule
{
    public sealed class RuleDocumentTests
    {
        [Fact]
        public void ReadDocument_Windows()
        {
            var document = GetDocument(GetToken(nx: false));
            var expected = GetExpected();

            Assert.Equal(expected.Name, document.Name);
            Assert.Equal(expected.Synopsis.Text, document.Synopsis.Text);
            Assert.Equal(expected.Recommendation.Text, document.Recommendation.Text);
            Assert.Equal(expected.Annotations["severity"], document.Annotations["severity"]);
            Assert.Equal(expected.Annotations["category"], document.Annotations["category"]);
        }

        [Fact]
        public void ReadDocument_Linux()
        {
            var document = GetDocument(GetToken(nx: true));
            var expected = GetExpected();

            Assert.Equal(expected.Name, document.Name);
            Assert.Equal(expected.Synopsis.Text, document.Synopsis.Text);
            Assert.Equal(expected.Recommendation.Text, document.Recommendation.Text);
            Assert.Equal(expected.Annotations["severity"], document.Annotations["severity"]);
            Assert.Equal(expected.Annotations["category"], document.Annotations["category"]);
        }

        private RuleDocument GetExpected()
        {
            var annotations = new Hashtable();
            annotations["severity"] = "Critical";
            annotations["category"] = "Pod security";

            var result = new RuleDocument(name: "Kubernetes.Deployment.NotLatestImage")
            {
                Synopsis = new TextBlock(text: "Containers should use specific tags instead of latest."),
                Annotations = TagSet.FromHashtable(annotations),
                Recommendation = new TextBlock(text: @"Deployments or pods should identify a specific tag to use for container images instead of latest. When latest is used it may be hard to determine which version of the image is running.
When using variable tags such as v1.0 (which may refer to v1.0.0 or v1.0.1) consider using imagePullPolicy: Always to ensure that the an out-of-date cached image is not used.
The latest tag automatically uses imagePullPolicy: Always instead of the default imagePullPolicy: IfNotPresent.")
            };

            return result;
        }

        private RuleDocument GetDocument(TokenStream stream)
        {
            var lexer = new RuleLexer();
            return lexer.Process(stream: stream);
        }

        private TokenStream GetToken(bool nx)
        {
            var reader = new MarkdownReader(yamlHeaderOnly: false);
            var content = GetMarkdownContent();

            if (nx)
            {
                content = content.Replace("\r\n", "\n");
            }
            else
            {
                if (!content.Contains("\r\n"))
                {
                    content = content.Replace("\n", "\r\n");
                }
            }

            return reader.Read(content, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RuleDocument.md"));
        }

        private string GetMarkdownContent()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RuleDocument.md");
            return File.ReadAllText(path);
        }
    }
}
