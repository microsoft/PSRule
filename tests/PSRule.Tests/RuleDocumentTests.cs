// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
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
            Assert.Equal(expected.Notes.Text, document.Notes.Text);
            Assert.Equal(expected.Annotations["severity"], document.Annotations["severity"]);
            Assert.Equal(expected.Annotations["category"], document.Annotations["category"]);
            Assert.Equal(expected.Links.Length, document.Links.Length);
            Assert.Equal(expected.Links[0].Name, document.Links[0].Name);
            Assert.Equal(expected.Links[0].Uri, document.Links[0].Uri);
            Assert.Equal(expected.Links[1].Name, document.Links[1].Name);
            Assert.Equal(expected.Links[1].Uri, document.Links[1].Uri);
        }

        [Fact]
        public void ReadDocument_Linux()
        {
            var document = GetDocument(GetToken(nx: true));
            var expected = GetExpected();

            Assert.Equal(expected.Name, document.Name);
            Assert.Equal(expected.Synopsis.Text, document.Synopsis.Text);
            Assert.Equal(expected.Recommendation.Text, document.Recommendation.Text);
            Assert.Equal(expected.Notes.Text, document.Notes.Text);
            Assert.Equal(expected.Annotations["severity"], document.Annotations["severity"]);
            Assert.Equal(expected.Annotations["category"], document.Annotations["category"]);
            Assert.Equal(expected.Links.Length, document.Links.Length);
            Assert.Equal(expected.Links[0].Name, document.Links[0].Name);
            Assert.Equal(expected.Links[0].Uri, document.Links[0].Uri);
            Assert.Equal(expected.Links[1].Name, document.Links[1].Name);
            Assert.Equal(expected.Links[1].Uri, document.Links[1].Uri);
        }

        private RuleDocument GetExpected()
        {
            var annotations = new Hashtable
            {
                ["severity"] = "Critical",
                ["category"] = "Security"
            };

            var links = new List<Link>
            {
                new Link { Name = "PSRule", Uri = "https://github.com/Microsoft/PSRule" },
                new Link { Name = "Stable tags", Uri = "https://docs.microsoft.com/en-us/azure/container-registry/container-registry-image-tag-version#stable-tags" }
            };

            var result = new RuleDocument(name: "Use specific tags")
            {
                Synopsis = new TextBlock(text: "Containers should use specific tags instead of latest."),
                Annotations = TagSet.FromHashtable(annotations),
                Recommendation = new TextBlock(text: @"Deployments or pods should identify a specific tag to use for container images instead of latest. When latest is used it may be hard to determine which version of the image is running.
When using variable tags such as v1.0 (which may refer to v1.0.0 or v1.0.1) consider using imagePullPolicy: Always to ensure that the an out-of-date cached image is not used.
The latest tag automatically uses imagePullPolicy: Always instead of the default imagePullPolicy: IfNotPresent."),
                Notes = new TextBlock(@"Test that [isIgnored].

{
    ""type"": ""Microsoft.Network/virtualNetworks"",
    ""name"": ""[parameters('VNETName')]"",
    ""apiVersion"": ""2020-06-01"",
    ""location"": ""[parameters('location')]"",
    ""properties"": {}
}"),
                Links = links.ToArray()
            };
            return result;
        }

        private RuleDocument GetDocument(TokenStream stream)
        {
            var lexer = new RuleLexer();
            return lexer.Process(stream);
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
                content = content.Replace("\r\n", "\n").Replace("\n", "\r\n");
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
