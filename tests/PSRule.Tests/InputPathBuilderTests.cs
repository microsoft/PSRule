// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using PSRule.Pipeline;

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
            Assert.True(actual.Length == 3);

            builder.Add("src/");
            actual = builder.Build();
            Assert.True(actual.Length > 100);

            // Check error handling
            var writer = new TestWriter(new Configuration.PSRuleOption());
            builder = new InputPathBuilder(writer, GetWorkingPath(), "*", null, null);
            builder.Add("ZZ://not/path");
            actual = builder.Build();
            Assert.Empty(actual);
            Assert.True(writer.Errors.Count(r => r.FullyQualifiedErrorId == "PSRule.ReadInputFailed") == 1);
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

        #region Helper methods

        private static string GetWorkingPath()
        {
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../.."));
        }

        #endregion Helper methods
    }
}
