// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using PSRule.Pipeline;

namespace PSRule
{
    public sealed class ModulePathComparerTests
    {
        [Fact]
        public void Sort()
        {
            var paths = new string[]
            {
                "C:/modules/PSRule/1.0.0",
                "C:/modules/PSRule/1.2.0",
                "C:/other/PSRule/1.0.0",
                "C:/modules/PSRule/10.0.0",
                "C:/other/PSRule/10.0.0",
                "C:/other/PSRule/1.2.0",
                "C:/other/PSRule/version",
                "C:/other/PSRule/0.1.0",
            };
            var comparer = new ModulePathComparer();
            Array.Sort(paths, comparer);

            Assert.Equal("C:/modules/PSRule/10.0.0", paths[0]);
            Assert.Equal("C:/other/PSRule/10.0.0", paths[1]);
            Assert.Equal("C:/modules/PSRule/1.2.0", paths[2]);
            Assert.Equal("C:/other/PSRule/1.2.0", paths[3]);
            Assert.Equal("C:/modules/PSRule/1.0.0", paths[4]);
            Assert.Equal("C:/other/PSRule/1.0.0", paths[5]);
            Assert.Equal("C:/other/PSRule/0.1.0", paths[6]);
            Assert.Equal("C:/other/PSRule/version", paths[7]);
        }
    }
}
