// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using System;
using Xunit;

namespace PSRule
{
    public sealed class PathFilterTests
    {
        [Fact]
        public void Match()
        {
            // Single expression match
            var filter = PathFilter.Create(GetWorkingPath(), "**/Resources.Parameters*.json");
            Assert.True(filter.Match("tests/example/Resources.Parameters.json"));
            Assert.True(filter.Match("tests/example2/Resources.Parameters2.json"));
            Assert.False(filter.Match("tests/example2/Resources.Parameters2.txt"));
            filter = PathFilter.Create(GetWorkingPath(), "**/example2/Resources.Parameters*.json");
            Assert.False(filter.Match("tests/example/Resources.Parameters.json"));
            Assert.True(filter.Match("tests/example2/Resources.Parameters2.json"));
            filter = PathFilter.Create(GetWorkingPath(), "**/example?/Resources.Parameters*.json");
            Assert.True(filter.Match("tests/example/Resources.Parameters.json"));
            Assert.True(filter.Match("tests/example2/Resources.Parameters2.json"));
            Assert.False(filter.Match("tests/example2/Resources.Parameters2.txt"));
            filter = PathFilter.Create(GetWorkingPath(), "tests/example?/Resources.Parameters.json");
            Assert.True(filter.Match("tests/example/Resources.Parameters.json"));
            Assert.True(filter.Match("tests/example2/Resources.Parameters.json"));
            filter = PathFilter.Create(GetWorkingPath(), "tests/example/Resources.Parameters*.json");
            Assert.True(filter.Match("tests/example/Resources.Parameters.json"));
            Assert.True(filter.Match("tests/example/Resources.Parameters2.json"));

            // Multi-expression match
            var expressions = new string[]
            {
                "out/",
                "**/bin/",
                "**/obj/",
                "",
                "# Add reports/bin",
                "!reports/bin/"
            };
            filter = PathFilter.Create(GetWorkingPath(), expressions, true);
            Assert.True(filter.Match("out/example.parameters.json"));
            Assert.False(filter.Match("outer/example.parameters.json"));
            Assert.False(filter.Match("reports/out/example.parameters.json"));
            Assert.True(filter.Match("src/bin/pwsh.exe"));
            Assert.True(filter.Match("src/obj/example.parameters.json"));
            Assert.False(filter.Match("reports/bin/other.json"));

            // Exclude
            filter = PathFilter.Create(GetWorkingPath(), expressions, false);
            Assert.False(filter.Match("out/example.parameters.json"));
            Assert.True(filter.Match("outer/example.parameters.json"));
            Assert.True(filter.Match("reports/out/example.parameters.json"));
            Assert.False(filter.Match("src/bin/pwsh.exe"));
            Assert.False(filter.Match("src/obj/example.parameters.json"));
            Assert.True(filter.Match("reports/bin/other.json"));
        }

        private static string GetWorkingPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}
