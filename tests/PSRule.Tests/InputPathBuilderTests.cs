// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using System;
using System.IO;
using Xunit;

namespace PSRule
{
    public sealed class InputPathBuilderTests
    {
        [Fact]
        public void GetPath()
        {
            var builder = new InputPathBuilder(null, GetWorkingPath(), "*", null);
            builder.Add(".");
            var actual = builder.Build();
            Assert.True(actual.Length > 100);

            builder.Add(GetWorkingPath());
            actual = builder.Build();
            Assert.True(actual.Length > 100);

            builder.Add("./src");
            actual = builder.Build();
            Assert.True(actual.Length == 0);

            builder.Add("./src/");
            actual = builder.Build();
            Assert.True(actual.Length > 100);

            builder.Add("./");
            actual = builder.Build();
            Assert.True(actual.Length > 100);

            builder.Add("./.azure-pipelines/*.yaml");
            actual = builder.Build();
            Assert.True(actual.Length == 1);

            builder.Add("./.azure-pipelines/**/*.yaml");
            actual = builder.Build();
            Assert.True(actual.Length == 3);

            builder.Add("./.azure-pipelines/");
            actual = builder.Build();
            Assert.True(actual.Length == 4);

            builder.Add(".azure-pipelines/");
            actual = builder.Build();
            Assert.True(actual.Length == 4);

            builder.Add("./*.json");
            actual = builder.Build();
            Assert.True(actual.Length == 1);

            builder.Add("src/");
            actual = builder.Build();
            Assert.True(actual.Length > 100);
        }

        private static string GetWorkingPath()
        {
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../.."));
        }
    }
}
