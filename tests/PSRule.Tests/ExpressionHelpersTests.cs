// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;

namespace PSRule
{
    public sealed class ExpressionHelpersTests
    {
        [Fact]
        public void WithinPath()
        {
            Assert.True(ExpressionHelpers.WithinPath("C:\\temp.json", "C:\\", caseSensitive: false));
            Assert.False(ExpressionHelpers.WithinPath("C:\\temp.json", "C:\\temp\\", caseSensitive: false));
        }

        [Fact]
        public void NormalizePath()
        {
            Assert.Equal("C:/temp.json", ExpressionHelpers.NormalizePath("C:\\longer\\directory\\name\\", "C:\\temp.json").Replace("/C:/", "C:/"));
        }
    }
}
