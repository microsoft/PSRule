// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule
{
    public sealed class StringExtensionsTests
    {
        [Fact]
        public void Replace()
        {
            var actual = "One Two Three";
            Assert.Equal("OneTwoThree", actual.Replace(" ", "", caseSensitive: true));
            Assert.Equal("OneTwoThree", actual.Replace(" ", "", caseSensitive: false));
            Assert.Equal("One Two ", actual.Replace("Three", "", caseSensitive: true));
            Assert.Equal("One Two ", actual.Replace("Three", "", caseSensitive: false));
            Assert.Equal(" Two Three", actual.Replace("One", "", caseSensitive: true));
            Assert.Equal(" Two Three", actual.Replace("One", "", caseSensitive: false));
            Assert.Equal("One 2 Three", actual.Replace("Two", "2", caseSensitive: true));
            Assert.Equal("One 2 Three", actual.Replace("Two", "2", caseSensitive: false));
            Assert.Equal("One Two Three", actual.Replace("two", "2", caseSensitive: true));
            Assert.Equal("One 2 Three", actual.Replace("two", "2", caseSensitive: false));
            Assert.Equal("One Two Three", actual.Replace("three", "3", caseSensitive: true));
            Assert.Equal("One Two 3", actual.Replace("three", "3", caseSensitive: false));
            Assert.Equal("One Two Three", actual.Replace("one", "1", caseSensitive: true));
            Assert.Equal("1 Two Three", actual.Replace("one", "1", caseSensitive: false));
        }
    }
}
