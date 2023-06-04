// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using PSRule.Pipeline;

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
                "!reports/bin/",
                "**/ObjectFromFile*.json",
                "!**/ObjectFromFile.json"
            };
            filter = PathFilter.Create(GetWorkingPath(), expressions, true);
            Assert.True(filter.Match("out/example.parameters.json"));
            Assert.False(filter.Match("outer/example.parameters.json"));
            Assert.False(filter.Match("reports/out/example.parameters.json"));
            Assert.True(filter.Match("src/bin/pwsh.exe"));
            Assert.True(filter.Match("src/obj/example.parameters.json"));
            Assert.False(filter.Match("reports/bin/other.json"));
            Assert.False(filter.Match("ObjectFromFile.json"));
            Assert.True(filter.Match("ObjectFromFileSingle.json"));

            // Exclude
            filter = PathFilter.Create(GetWorkingPath(), expressions, false);
            Assert.False(filter.Match("out/example.parameters.json"));
            Assert.True(filter.Match("outer/example.parameters.json"));
            Assert.True(filter.Match("reports/out/example.parameters.json"));
            Assert.False(filter.Match("src/bin/pwsh.exe"));
            Assert.False(filter.Match("src/obj/example.parameters.json"));
            Assert.True(filter.Match("reports/bin/other.json"));
        }

        [Fact]
        public void Builder()
        {
            var builder = PathFilterBuilder.Create(GetWorkingPath(), new string[] { "out/" }, false, false);
            var actual1 = builder.Build();
            Assert.False(actual1.Match("out/not.file"));
            Assert.True(actual1.Match(".git/HEAD"));
            Assert.True(actual1.Match(".gitignore"));
            Assert.True(actual1.Match(".github/CODEOWNERS"));
            Assert.True(actual1.Match(".github/dependabot.yml"));

            builder = PathFilterBuilder.Create(GetWorkingPath(), new string[] { "out/" }, true, false);
            var actual2 = builder.Build();
            Assert.False(actual2.Match("out/not.file"));
            Assert.False(actual2.Match(".git/HEAD"));
            Assert.True(actual2.Match(".gitignore"));
            Assert.True(actual2.Match(".github/CODEOWNERS"));
            Assert.True(actual2.Match(".github/dependabot.yml"));

            builder = PathFilterBuilder.Create(GetWorkingPath(), new string[] { "out/" }, true, true);
            var actual3 = builder.Build();
            Assert.False(actual3.Match("out/not.file"));
            Assert.False(actual3.Match(".git/HEAD"));
            Assert.False(actual3.Match(".gitignore"));
            Assert.False(actual3.Match(".github/CODEOWNERS"));
            Assert.True(actual3.Match(".github/dependabot.yml"));
        }

        #region Helper methods

        private static string GetWorkingPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        #endregion Helper methods
    }
}
