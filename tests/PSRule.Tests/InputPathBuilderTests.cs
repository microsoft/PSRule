// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using PSRule.Pipeline;
using Xunit;

namespace PSRule
{
    public sealed class InputPathBuilderTests
    {
        [Fact]
        public void GetPath()
        {
            var builder = new InputPathBuilder(null, GetWorkingPath(), "*", null, null);
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

            builder.Add("./.github/*.yaml");
            actual = builder.Build();
            Assert.Single(actual);

            builder.Add("./.github/**/*.yaml");
            actual = builder.Build();
            Assert.Equal(7, actual.Length);

            builder.Add("./.github/");
            actual = builder.Build();
            Assert.Equal(13, actual.Length);

            builder.Add(".github/");
            actual = builder.Build();
            Assert.Equal(13, actual.Length);

            builder.Add("./*.json");
            actual = builder.Build();
            Assert.True(actual.Length == 2);

            builder.Add("src/");
            actual = builder.Build();
            Assert.True(actual.Length > 100);
        }

        [Fact]
        public void GetPathRequired()
        {
            var required = PathFilter.Create(GetWorkingPath(), new string[] { "README.md" });
            var builder = new InputPathBuilder(null, GetWorkingPath(), "*", null, required);
            builder.Add(".");
            var actual = builder.Build();
            Assert.True(actual.Length == 1);

            builder.Add(GetWorkingPath());
            actual = builder.Build();
            Assert.True(actual.Length == 1);

            required = PathFilter.Create(GetWorkingPath(), new string[] { "**" });
            builder = new InputPathBuilder(null, GetWorkingPath(), "*", null, required);
            builder.Add(".");
            actual = builder.Build();
            Assert.True(actual.Length > 100);
        }

        private static string GetWorkingPath()
        {
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../.."));
        }
    }
}
